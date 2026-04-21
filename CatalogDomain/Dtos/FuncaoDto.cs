using System.ComponentModel.DataAnnotations;

namespace CatalogDomain.Dtos;

public class FuncaoDto
{
    public string CodFuncao { get; set; } = default!;
    public string CodServico { get; set; } = default!;
    public string Label { get; set; } = default!;
    public string? Descricao { get; set; }
    public string? Icone { get; set; }
    public int NumOrdem { get; set; }
    public bool IndAtivo { get; set; }
}

public class FuncaoRequest
{
    [Required(ErrorMessage = "Codigo da funcao e obrigatorio.")]
    [StringLength(50, ErrorMessage = "Codigo da funcao deve ter no maximo 50 caracteres.")]
    public string CodFuncao { get; set; } = null!;

    [Required(ErrorMessage = "Label da funcao e obrigatorio.")]
    [StringLength(100, ErrorMessage = "Label deve ter no maximo 100 caracteres.")]
    public string Label { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Descricao deve ter no maximo 500 caracteres.")]
    public string? Descricao { get; set; }

    [StringLength(100, ErrorMessage = "Icone deve ter no maximo 100 caracteres.")]
    public string? Icone { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Numero de ordem não pode ser negativo.")]
    public int NumOrdem { get; set; }

    public bool IndAtivo { get; set; }
}