using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ScannerKeyHunt.Domain.ModelDTOs
{
    public class PostsViewModelDTO : BaseModelDTO
    {
        public PostsViewModelDTO() : base()
        {
            UUID = UUID == null ? Guid.NewGuid() : UUID;
            CreationDate = CreationDate == null ? DateTime.UtcNow : CreationDate;
            UpdateDate = UpdateDate == null ? DateTime.UtcNow : UpdateDate;
            DataCadastro = DateTime.UtcNow;
        }

        public PostsViewModelDTO(
            string titulo,
            string descricao,
            string imagemUrl,
            IFormFile imagem,
            long id,
            DateTime? deletionDate
        ) : base(id, deletionDate)
        {
            Id = id;
            UUID = UUID == null ? Guid.NewGuid() : UUID;
            CreationDate = CreationDate == null ? DateTime.UtcNow : CreationDate;
            UpdateDate = UpdateDate == null ? DateTime.UtcNow : UpdateDate;
            DeletionDate = deletionDate;

            Titulo = titulo;
            Descricao = descricao;
            DataCadastro = DateTime.UtcNow;
            ImagemUrl = imagemUrl;
            Imagem = imagem;
        }

        [Required(ErrorMessage = "O campo Titulo é obrigatório")]
        [MaxLength(150, ErrorMessage = "O titulo suporta no máximo 150 caracteres")]
        public string Titulo { get; set; }
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public string Descricao { get; set; }
        public DateTime DataCadastro { get; set; }
        public string DataCadastroFormatada { get { return DataCadastro.ToString("dd/MM/yyyy"); } }
        public string ImagemUrl { get; set; }
        public IFormFile Imagem { get; set; }
        public string NovoCampoTeste { get { return Titulo + "-" + Descricao; } }
    }
}
