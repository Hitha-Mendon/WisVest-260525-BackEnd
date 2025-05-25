using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WisVestAPI.Constants;
using WisVestAPI.Models;
using WisVestAPI.Models.DTOs;
using WisVestAPI.Models.Matrix;
using WisVestAPI.Services;

namespace WisVestAPI.Controllers
{
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
private readonly ILogger<ProductsController> _logger;
private readonly ProductRepositoryService _productRepository;
    public ProductsController(ILogger<ProductsController> logger, ProductRepositoryService productRepository)
    {
        _logger = logger;
        _productRepository = productRepository;
    }

    [HttpGet("products")]
    public async Task<IActionResult> LoadProducts()
    {
        try
        {
            var productData = await _productRepository.LoadProductMatrixAsync();

            if (productData == null)
            {
                _logger.LogWarning(ResponseMessages.ProductJsonNotFound);
                return NotFound(ResponseMessages.ProductJsonNotFound);
            }

            var products = productData.Values
                                      .SelectMany(asset => asset.Values)
                                      .SelectMany(subAsset => subAsset)
                                      .ToList();

            var productDTOs = products.Select(p => new ProductDTO
            {
                ProductName = p.ProductName,
                AnnualReturn = p.AnnualReturn,
                AssetClass = p.AssetClass,
                SubAssetClass = p.SubAssetClass,
                Liquidity = p.Liquidity,
                Pros = p.Pros,
                Cons = p.Cons,
                RiskLevel = p.RiskLevel,
                description = p.description
            }).ToList();

            return Ok(productDTOs);
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, ResponseMessages.JsonReadError);
            return StatusCode(500, new { message = ResponseMessages.JsonReadError, error = ex.Message });
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, ResponseMessages.UnexpectedError);
            return StatusCode(500, new { message = ResponseMessages.UnexpectedError, error = ex.Message });
        }
    }
}
}