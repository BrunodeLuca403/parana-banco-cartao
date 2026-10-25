namespace ParanaBanco.Cartoes.Application.Common
{
    public record Error
    {
        public static Error None => new(string.Empty, string.Empty, ErrorTypeEnum.Failure);

        private Error(string code, string description, ErrorTypeEnum type)
        {
            Code = code;
            Description = description;
            Type = type;
        }

        public string Code { get; init; }
        public string Description { get; init; }
        public ErrorTypeEnum Type { get; init; }

        public static Error Validation(string code, string description) => new(code, description, ErrorTypeEnum.Validation);
        public static Error NotFound(string code, string description) => new(code, description, ErrorTypeEnum.NotFound);
        public static Error Conflict(string code, string description) => new(code, description, ErrorTypeEnum.Conflict);
        public static Error Failure(string code, string description) => new(code, description, ErrorTypeEnum.Failure);
    }
}
