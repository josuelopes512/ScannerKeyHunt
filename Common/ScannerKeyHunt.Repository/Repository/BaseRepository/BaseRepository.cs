using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScannerKeyHunt.Data.Context;
using ScannerKeyHunt.Repository.Interfaces;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;

namespace ScannerKeyHunt.Repository.Repository.BaseRepository
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        private readonly BaseContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public BaseRepository(BaseContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public bool IsMarkedForDeletion(TEntity entity)
        {
            var deletionDateProperty = entity.GetType().GetProperty("DeletionDate");
            var deletionDate = deletionDateProperty?.GetValue(entity);

            return deletionDate != null;
        }

        public virtual void Add(TEntity entity)
        {
            typeof(TEntity).GetProperty("Id").SetValue(entity, Guid.NewGuid());
            typeof(TEntity).GetProperty("CreationDate").SetValue(entity, DateTime.UtcNow);
            typeof(TEntity).GetProperty("UpdateDate").SetValue(entity, DateTime.UtcNow);

            _dbSet.Add(entity);
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            DateTime currentTime = DateTime.UtcNow;
            PropertyInfo? idProperty = typeof(TEntity).GetProperty("Id");
            PropertyInfo? creationDateProperty = typeof(TEntity).GetProperty("CreationDate");
            PropertyInfo? updateDateProperty = typeof(TEntity).GetProperty("UpdateDate");

            foreach (var entity in entities)
            {
                idProperty.SetValue(entity, Guid.NewGuid());
                creationDateProperty.SetValue(entity, currentTime);
                updateDateProperty.SetValue(entity, currentTime);
            }

            _dbSet.AddRange(entities);
        }

        public List<TEntity> GetAll()
        {
            try
            {
                return ListEntities();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<TEntity> GetAll(Predicate<TEntity> predicate)
        {
            try
            {
                return _dbSet.AsEnumerable()
                     .Where(e => !IsMarkedForDeletion(e))  // Aplicar lógica local após carregar os dados
                     .Where(e => predicate(e))             // Aplicar o predicate
                     .ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool Exists(Predicate<TEntity> predicate)
        {
            try
            {
                return _dbSet.AsNoTracking()
                    .Any(e => predicate(e));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<TEntity> GetAllDeleted()
        {
            try
            {
                return _dbSet.AsEnumerable().Where(item => IsMarkedForDeletion(item)).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<TEntity> ListEntities()
        {
            try
            {
                return _dbSet.AsEnumerable().Where(item => !IsMarkedForDeletion(item)).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public TEntity GetByGuid(Guid guid)
        {
            try
            {
                return _dbSet.AsEnumerable().Where(e => (Guid)e.GetType().GetProperty("Id").GetValue(e) == guid && e.GetType().GetProperty("DeletionDate").GetValue(e) == null).FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int GetCount()
        {
            try
            {
                return _dbSet.AsNoTracking().Count();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public TEntity GetByExpressionBool(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                return _dbSet.AsNoTracking().SingleOrDefault(predicate);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<TEntity> GetWithRawSql(string query, params object[] parameters)
        {
            try
            {
                return _dbSet.FromSqlRaw(query, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<TEntity> GetPages<Tipo>(int numPage, int qtdRegs) where Tipo : class
        {
            try
            {
                return _dbSet.AsEnumerable()
                    .Where(e => !IsMarkedForDeletion(e))
                    .Skip(qtdRegs * (numPage - 1))
                      .Take(qtdRegs).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<TEntity> GetPages<Tipo>(Predicate<TEntity> predicate, int numPage, int qtdRegs) where Tipo : class
        {
            try
            {
                return _dbSet.AsEnumerable()
                    .Where(e => predicate(e))
                    .Where(e => !IsMarkedForDeletion(e))
                    .Skip(qtdRegs * (numPage - 1))
                      .Take(qtdRegs).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int GetCount<Tipo>() where Tipo : class
        {
            try
            {
                return _dbSet.AsNoTracking().Count();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int GetCount<Tipo>(Predicate<TEntity> predicate) where Tipo : class
        {
            try
            {
                return _dbSet.AsEnumerable()
                    .Where(e => !IsMarkedForDeletion(e))
                    .Where(e => predicate(e))
                    .Count();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual void Update(TEntity entity)
        {
            try
            {
                TEntity existingEntity = _dbSet.Find(typeof(TEntity).GetProperty("Id").GetValue(entity));

                if (existingEntity != null)
                {
                    typeof(TEntity).GetProperty("UpdateDate").SetValue(existingEntity, DateTime.UtcNow);

                    _context.Entry(existingEntity).CurrentValues.SetValues(entity);

                    _context.Entry(existingEntity).State = EntityState.Modified;

                    _context.SaveChanges();
                }
                else
                {
                    throw new InvalidOperationException("Entidade não encontrada no banco de dados.");
                }
            }
            catch (Exception ex)
            {
                // Log de exceção, se necessário
                throw new Exception("Erro ao atualizar a entidade.", ex);
            }
        }

        public void Update(Guid guid, TEntity entity)
        {
            try
            {
                TEntity existingEntity = GetByGuid(guid);

                if (existingEntity != null)
                {
                    foreach (var property in typeof(TEntity).GetProperties())
                    {
                        if (property.Name.Equals("UpdateDate"))
                            property.SetValue(existingEntity, DateTime.UtcNow);

                        else if (property.Name.Equals("CreationDate"))
                            continue;

                        else if (property.Name != "Id")
                            property.SetValue(existingEntity, property.GetValue(entity));
                    }

                    _context.Entry(existingEntity).State = EntityState.Modified;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Delete(Guid guid)
        {
            try
            {
                TEntity existingEntity = GetByGuid(guid);

                bool isDeleted = existingEntity.GetType().GetProperty("DeletionDate").GetValue(existingEntity) == null;

                if (existingEntity != null && isDeleted)
                {
                    typeof(TEntity).GetProperty("DeletionDate").SetValue(existingEntity, DateTime.UtcNow);
                    typeof(TEntity).GetProperty("UpdateDate").SetValue(existingEntity, DateTime.UtcNow);

                    _context.Entry(existingEntity).State = EntityState.Modified;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual void Delete(TEntity entity)
        {
            try
            {
                Guid guid = (Guid)entity.GetType().GetProperty("Id").GetValue(entity);

                Delete(guid);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Delete(Guid guid, TEntity entity)
        {
            try
            {
                if (!entity.GetType().GetProperty("Id").GetValue(entity).Equals(guid))
                    throw new Exception("UUID value is different from entity uuid value");

                Delete(guid);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Remove(TEntity entity)
        {
            try
            {
                _dbSet.Remove(entity);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ExecuteSqlCommand(string query, params object[] parameters)
        {
            try
            {
                dynamic data = _context.Database.ExecuteSqlRaw(query, parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertData(params object[] parameters)
        {
            string columns = string.Join(", ", parameters.Select((param, _) => $"[{param.ToString().Split("param").ToList().Last()}]"));
            string values = string.Join(", ", parameters.Select((param, _) => $"{param}"));

            string query = $"INSERT INTO {typeof(TEntity).Name}s ({columns}) VALUES ({values})";

            _context.Database.ExecuteSqlRaw(query, parameters);
        }

        public void UpdateData(params object[] parameters)
        {
            string values = string.Join(", ", parameters
                .Where((param, _) => !param.ToString().Contains("Id"))
                .Select((param, _) => param.ToString().Split("param").ToList().Last() + " = " + param));

            string query = $"UPDATE {typeof(TEntity).Name}s SET {values} WHERE Id = @paramId";

            _context.Database.ExecuteSqlRaw(query, parameters);
        }

        public void BulkUpdate(List<TEntity> entities, bool isDeletion = false)
        {
            var entity = entities.FirstOrDefault();
            var updateUserId = entity?.GetType()?.GetProperty("UpdateUserId")?.GetValue(entity);

            List<SqlParameter> sqlParameters = new List<SqlParameter>
            {
                new SqlParameter("@paramUpdateDate", DateTime.UtcNow),
                new SqlParameter("@paramUpdateUserId", updateUserId ?? Guid.Empty),
            };

            if (isDeletion)
                sqlParameters.Add(new SqlParameter("@paramDeletionDate", DateTime.UtcNow));

            const int batchSize = 2000;

            List<(SqlParameter[], SqlParameter[])> batches = new List<(SqlParameter[], SqlParameter[])>();

            for (int i = 0; i < entities.Count; i += batchSize)
            {
                List<TEntity> batch = entities.Skip(i).Take(batchSize).ToList();

                List<SqlParameter> idParameters = batch.Select((x, index) => new SqlParameter($"@id{index}", (Guid)x.GetType().GetProperty("Id").GetValue(x))).ToList();

                batches.Add((idParameters.ToArray(), sqlParameters.ToArray()));
            }

            batches.ForEach((batch) => BulkUpdate(batch.Item1, batch.Item2));
        }

        private void BulkUpdate(SqlParameter[] parametersIds, params object[] parameters)
        {
            string values = string.Join(", ", parameters
                .Where((param, _) => !param.ToString().Contains("Id"))
                .Select((param, _) => param.ToString().Split("param").ToList().Last() + " = " + param));

            string entitiesIds = string.Join(", ", parametersIds.Select(p => p.ParameterName));

            string query = $"UPDATE {typeof(TEntity).Name}s SET {values} WHERE [Id] IN ({entitiesIds})";

            object[] sqlParameters = parametersIds.Concat(parameters).ToArray();

            _context.Database.ExecuteSqlRaw(query, sqlParameters);
        }

        public bool Exists(Guid guid)
        {
            try
            {
                return _dbSet.AsNoTracking()
                    .Any(e => (Guid)e.GetType().GetProperty("Id").GetValue(e) == guid);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<TEntity> GetAll(Guid userId)
        {
            try
            {
                return _dbSet.AsEnumerable()
                    .Where(item => !IsMarkedForDeletion(item))
                    .Where(x => (Guid)x.GetType().GetProperty("UserId").GetValue(x) == userId).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<T> RawSqlQuery<T>(string query, Func<DbDataReader, T> map, bool isStoredProcedure = false)
        {
            using (DbCommand command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = isStoredProcedure ? CommandType.StoredProcedure : CommandType.Text;

                _context.Database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    var entities = new List<T>();

                    while (result.Read())
                    {
                        entities.Add(map(result));
                    }

                    return entities;
                }
            }
        }

        public List<T> ExecuteQuery<T>(string query) where T : class, new()
        {
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                _context.Database.OpenConnection();

                using (var reader = command.ExecuteReader())
                {
                    var lst = new List<T>();
                    var lstColumns = new T().GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();

                    while (reader.Read())
                    {
                        var newObject = new T();
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            var name = reader.GetName(i);
                            PropertyInfo prop = lstColumns.FirstOrDefault(a => a.Name.ToLower().Equals(name.ToLower()));
                            if (prop == null)
                            {
                                continue;
                            }
                            var val = reader.IsDBNull(i) ? null : reader[i];
                            prop.SetValue(newObject, val, null);
                        }
                        lst.Add(newObject);
                    }

                    return lst;
                }
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
