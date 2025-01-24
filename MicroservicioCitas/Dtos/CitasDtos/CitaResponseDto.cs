public class CitaResponseDto {

    public int Id { get; set; }
    public string? PacienteId { get; set; }
    public string? MedicoId { get; set; }
    public DateTime Fecha { get; set; }
    public string? Lugar { get; set; }
    public string? Estado { get; set; } // "Pendiente", "En proceso", "Finalizada" 
}