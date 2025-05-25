using Microsoft.AspNetCore.Mvc;
using WisVestAPI.Models.DTOs;
using WisVestAPI.Services.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using WisVestAPI.Constants; // for ResponseMessages

namespace WisVestAPI.Controllers
{
    // [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AllocationController : ControllerBase
    {
        private readonly IAllocationService _allocationService;

        public AllocationController(IAllocationService allocationService)
        {
            _allocationService = allocationService;
        }

        // POST: api/Allocation/compute
        [HttpPost("compute")]
        public async Task<ActionResult<AllocationResultDTO>> GetAllocation([FromBody] UserInputDTO input)
        {
            if (input == null)
            {
                return BadRequest(ResponseMessages.InputCannotBeNull);
            }

            var fullAllocationResult = await _allocationService.CalculateFinalAllocation(input);

            if (fullAllocationResult == null || !fullAllocationResult.ContainsKey("assets"))
            {
                return BadRequest(ResponseMessages.AllocationNotComputed);
            }

            var assetsData = fullAllocationResult["assets"] as Dictionary<string, object>;
            if (assetsData == null)
            {
                return StatusCode(500, ResponseMessages.AllocationDataFormatIncorrect);
            }

            var result = new AllocationResultDTO { Assets = new Dictionary<string, AssetAllocation>() };

            foreach (var assetPair in assetsData)
            {
                var assetName = assetPair.Key;
                if (assetPair.Value is Dictionary<string, object> assetDetails)
                {
                    var assetAllocation = ParseAssetDetails(assetDetails);
                    if (assetAllocation != null)
                    {
                        result.Assets[assetName] = assetAllocation;
                    }
                }
            }

            return Ok(result);
        }

        [HttpGet("get-calculated-allocations")]
        public async Task<IActionResult> GetCalculatedAllocations()
        {
            try
            {
                var finalAllocation = await _allocationService.GetFinalAllocationFromFileAsync();

                if (finalAllocation == null || !finalAllocation.ContainsKey("assets"))
                {
                    return NotFound(ResponseMessages.NoCalculatedAllocationsFound);
                }

                return Ok(finalAllocation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ResponseMessages.AllocationRetrievalError, error = ex.Message });
            }
        }

        private AssetAllocation? ParseAssetDetails(Dictionary<string, object> assetDetails)
        {
            if (assetDetails.TryGetValue("percentage", out var percentageObj) &&
                assetDetails.TryGetValue("subAssets", out var subAssetsObj) &&
                percentageObj is double percentage &&
                subAssetsObj is Dictionary<string, double> subAssets)
            {
                return new AssetAllocation
                {
                    Percentage = percentage,
                    SubAssets = subAssets
                };
            }
            return null;
        }
    }
}