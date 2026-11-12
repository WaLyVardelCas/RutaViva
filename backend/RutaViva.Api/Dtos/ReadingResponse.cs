using System.ComponentModel;
using RutaViva.Application;

namespace RutaViva.Api;

/// <summary>Resultado de simular una lectura de telemetría.</summary>
public sealed record ReadingResponse(
    [property: Description("Accepted si el kilometraje se aplicó. Ignored si no supera el actual o si el eventId ya fue procesado.")]
    ReadingResult Result);
