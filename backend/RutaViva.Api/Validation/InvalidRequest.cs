using Microsoft.AspNetCore.Mvc;

namespace RutaViva.Api;

public static class InvalidRequest
{
    public static BadRequestObjectResult For(string detail) =>
        new(new ApiError("Datos inválidos", detail));
}
