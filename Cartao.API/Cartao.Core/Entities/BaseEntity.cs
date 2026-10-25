namespace ParanaBanco.Cartoes.Core.Entities
{
    public abstract class BaseEntity
    {
        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CriadoEm = DateTime.UtcNow;
        }

        public Guid Id { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}
