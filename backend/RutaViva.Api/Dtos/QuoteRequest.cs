using System.ComponentModel;

namespace RutaViva.Api;

/// <summary>Cotización de una orden.</summary>
public sealed record QuoteRequest(
    [property: Description("Monto en COP. Obligatorio y mayor que cero. Hasta 1.500.000 la orden queda Approved; por encima queda Quoted.")] decimal? Amount);
