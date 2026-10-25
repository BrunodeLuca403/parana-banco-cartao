namespace ParanaBanco.Cartoes.Core.Entities
{
    public class BaseEntity
    {
        public BaseEntity()
        {
            Id = Id = Guid.NewGuid();
            CreatAt = DateTime.Now;
            IsDelete = false;
        }

        public Guid Id { get; set; }
        public DateTime CreatAt { get; set; }
        public bool IsDelete { get; set; }


        public void SetAsDelete()
        {
            IsDelete = true;
        }
    }
}
