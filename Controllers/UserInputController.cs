using Microsoft.AspNetCore.Mvc;
using WisVestAPI.Models.DTOs;
using WisVestAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using WisVestAPI.Constants;
using System.Linq;
using System.Threading.Tasks;

namespace WisVestAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserInputController : ControllerBase
    {
        private readonly IUserInputService _userInputService;
        private readonly ILogger<UserInputController> _logger;

        public UserInputController(IUserInputService userInputService, ILogger<UserInputController> logger)
        {
            _userInputService = userInputService;
            _logger = logger;
        }

        [HttpPost("submit-input")]
        public async Task<IActionResult> SubmitInput([FromBody] UserInputDTO input)
        {
            if (input == null)
            {
                _logger.LogWarning(ResponseMessages.LogNullInputReceived);
                return BadRequest(new { message = ResponseMessages.NullInput });
            }

            if (input.Age < AppConstants.MinAge || input.Age > AppConstants.MaxAge)
            {
                _logger.LogWarning(ResponseMessages.LogInvalidAge, input.Age);
                return BadRequest(new 
                { 
                    message = input.Age < AppConstants.MinAge ? ResponseMessages.InvalidAgeUnder : ResponseMessages.InvalidAgeOver 
                });
            }

            if (input.InvestmentHorizon < AppConstants.MinInvestmentHorizon || input.InvestmentHorizon > AppConstants.MaxInvestmentHorizon)
            {
                _logger.LogWarning(ResponseMessages.LogInvalidInvestmentHorizon, input.InvestmentHorizon);
                return BadRequest(new
                {
                    message = input.InvestmentHorizon < AppConstants.MinInvestmentHorizon ? ResponseMessages.InvalidInvestmentHorizonUnder : ResponseMessages.InvalidInvestmentHorizonOver
                });
            }

            if (string.IsNullOrEmpty(input.RiskTolerance) || !AppConstants.ValidRiskTolerances.Contains(input.RiskTolerance))
            {
                _logger.LogWarning(ResponseMessages.LogInvalidRiskTolerance, input.RiskTolerance);
                return BadRequest(new { message = ResponseMessages.InvalidRiskTolerance });
            }

            if (input.TargetAmount < AppConstants.MinTargetAmount || input.TargetAmount > AppConstants.MaxTargetAmount)
            {
                _logger.LogWarning(ResponseMessages.LogInvalidTargetAmount, input.TargetAmount);
                return BadRequest(new
                {
                    message = input.TargetAmount < AppConstants.MinTargetAmount ? ResponseMessages.InvalidTargetAmountUnder : ResponseMessages.InvalidTargetAmountOver
                });
            }

            try
            {
                var result = await _userInputService.HandleUserInput(input);
                return Ok(result);
            }
            catch (System.ArgumentException ex)
            {
                _logger.LogWarning(ex, ResponseMessages.LogInvalidInputData);
                return BadRequest(new { message = ex.Message });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, ResponseMessages.LogInternalServerError);
                return StatusCode(500, new { message = ResponseMessages.InternalServerError, error = ex.Message });
            }
        }
    }
}