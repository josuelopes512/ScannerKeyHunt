using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ScannerKeyHunt.Data.Context;
using ScannerKeyHunt.Data.Entities;
using ScannerKeyHunt.Repository.Interfaces.Repository;
using ScannerKeyHunt.Repository.Repository.BaseRepository;

namespace ScannerKeyHunt.Repository.Repository
{
    public class TokenAuthRepository : BaseRepository<TokenAuth>, ITokenAuthRepository
    {
        private readonly BaseContext _context;
        public TokenAuthRepository(BaseContext context) : base(context)
        {
            _context = context;
        }

        public override void Add(TokenAuth entity)
        {
            try
            {
                entity.Id = Guid.NewGuid();

                object[] sqlParameters = new object[]
                {
                    new SqlParameter("@paramId", entity.Id),
                    new SqlParameter("@paramUserId", entity.UserId.ToString()),
                    new SqlParameter("@paramRefreshToken", entity.RefreshToken),
                    new SqlParameter("@paramExpirationDate", entity.ExpirationDate),
                    new SqlParameter("@paramIsActive", true),
                    new SqlParameter("@paramCreationDate", DateTime.UtcNow),
                    new SqlParameter("@paramUpdateDate", DateTime.UtcNow),
                    new SqlParameter("@paramCreationUserId", entity.CreationUserId),
                    new SqlParameter("@paramUpdateUserId", entity.UpdateUserId ?? Guid.Empty),
                    new SqlParameter("@paramDeletionDate", (object)DBNull.Value),
                };

                InsertData(sqlParameters);
            }
            catch (Exception)
            {
                base.Add(entity);
            }
        }

        public override void Update(TokenAuth entity)
        {
            try
            {
                object[] sqlParameters = new object[]
                {
                    new SqlParameter("@paramId", entity.Id),
                    new SqlParameter("@paramUserId", entity.UserId.ToString()),
                    new SqlParameter("@paramRefreshToken", entity.RefreshToken),
                    new SqlParameter("@paramExpirationDate", entity.ExpirationDate),
                    new SqlParameter("@paramIsActive", true),
                    new SqlParameter("@paramCreationDate", DateTime.UtcNow),
                    new SqlParameter("@paramUpdateDate", DateTime.UtcNow),
                    new SqlParameter("@paramCreationUserId", entity.CreationUserId),
                    new SqlParameter("@paramUpdateUserId", entity.UpdateUserId ?? Guid.Empty),
                    new SqlParameter("@paramDeletionDate", (object)DBNull.Value),
                };

                UpdateData(sqlParameters);
            }
            catch (Exception)
            {
                base.Update(entity);
            }
        }

        public override void Delete(TokenAuth entity)
        {
            try
            {
                object[] sqlParameters = new object[]
                {
                    new SqlParameter("@paramId", entity.Id),
                    new SqlParameter("@paramUpdateDate", DateTime.UtcNow),
                    new SqlParameter("@paramUpdateUserId", entity.UpdateUserId ?? Guid.Empty),
                    new SqlParameter("@paramDeletionDate", DateTime.UtcNow),
                };

                UpdateData(sqlParameters);
            }
            catch (Exception)
            {
                base.Delete(entity);
            }
        }

        public TokenAuth GetTokenAuthByUserId(Guid userId)
        {
            try
            {
                string query = "SELECT * FROM [TokenAuths] (NOLOCK)";

                return _context.TokenAuths.FromSqlRaw(query)
                    .Where(x => x.UserId == userId && x.IsActive && x.DeletionDate == null)
                    .OrderByDescending(x => x.Id).FirstOrDefault();
            }
            catch (Exception)
            {
                return base.GetAll().FindAll(x => x.UserId == userId).FirstOrDefault();
            }
        }
    }
}
