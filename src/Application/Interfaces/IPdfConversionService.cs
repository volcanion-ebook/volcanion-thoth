using Application.DTOs;
using Domain.ValueObjects;

namespace Application.Interfaces;

public interface IPdfConversionService
{
    Task<ConversionResponseDto> ConvertPdfAsync(PdfUploadDto uploadDto);
    List<string> GetSupportedFormats();
}
