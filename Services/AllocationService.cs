using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WisVestAPI.Constants;
using WisVestAPI.Models.DTOs;
using WisVestAPI.Models.Matrix;
using WisVestAPI.Repositories.Matrix;
using WisVestAPI.Services.Interfaces;
 
namespace WisVestAPI.Services
{
    public class AllocationService : IAllocationService
    {
        private readonly MatrixRepository _matrixRepository;
        private readonly ILogger<AllocationService> _logger;

        public AllocationService(MatrixRepository matrixRepository, ILogger<AllocationService> logger)
        {
            _matrixRepository = matrixRepository;
            _logger = logger;
        }

        private async Task SaveFinalAllocationToFileAsync(Dictionary<string, object> finalAllocation)       
        {
            try
            {
                var filePath = AppConstants.FinalAllocationFilePath;
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(finalAllocation, options);
                await File.WriteAllTextAsync(filePath, jsonString);
                _logger.LogInformation(ResponseMessages.FinalAllocationSaved, filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ResponseMessages.AllocationFileSaveError);
                throw;
            }
        }

        public async Task<Dictionary<string, object>> CalculateFinalAllocation(UserInputDTO input)
        {  
             try{
                _logger.LogInformation(ResponseMessages.AllocationCalculationStarted);

                var allocationMatrix = await _matrixRepository.LoadMatrixDataAsync();
                if (allocationMatrix == null)
                {
                    _logger.LogError(ResponseMessages.AllocationMatrixNull);
                    throw new InvalidOperationException(ResponseMessages.AllocationMatrixNull);
                }

                _logger.LogInformation(ResponseMessages.AllocationMatrixLoaded);

                var riskToleranceMap = new Dictionary<string, string>
                {
                    { AppConstants.RiskLow, AppConstants.RiskLow },
                    { AppConstants.RiskMedium, AppConstants.RiskMidMapped },
                    { AppConstants.RiskHigh, AppConstants.RiskHigh }
                };

                var investmentHorizonMap = new Dictionary<string, string>
                {
                    { AppConstants.HorizonShort, AppConstants.HorizonShortMapped },
                    { AppConstants.HorizonModerate, AppConstants.HorizonModerateMapped },
                    { AppConstants.HorizonLong, AppConstants.HorizonLongMapped }
                };

                if (input.InvestmentHorizon <= 0)
                    throw new ArgumentException(ResponseMessages.HorizonMissing);

                var riskToleranceKey = riskToleranceMap[input.RiskTolerance ?? throw new ArgumentException(ResponseMessages.RiskToleranceMissing)];
                var horizonGroup = GetHorizonGroup(input.InvestmentHorizon);
                var investmentHorizonKey = investmentHorizonMap[horizonGroup];
                var riskHorizonKey = $"{riskToleranceKey}+{investmentHorizonKey}";
    
                _logger.LogInformation(ResponseMessages.BaseAllocationLookup, riskHorizonKey);

                // Step 2: Determine base allocation
                if (!allocationMatrix.Risk_Horizon_Allocation.TryGetValue(riskHorizonKey, out var baseAllocation))
                {
                    _logger.LogError(ResponseMessages.BaseAllocationNotFound, riskHorizonKey);
                    throw new ArgumentException(string.Format(ResponseMessages.BaseAllocationInvalidCombo, riskHorizonKey));
                }

                _logger.LogInformation(ResponseMessages.BaseAllocationFound, JsonSerializer.Serialize(baseAllocation));
       
                // Clone the base allocation to avoid modifying the original matrix
                var finalAllocation = new Dictionary<string, double>(baseAllocation);
                try{
                // Step 3: Apply age adjustment rules
                    var ageRuleKey = GetAgeGroup(input.Age);
                    _logger.LogInformation(ResponseMessages.AgeAdjustmentLookup, ageRuleKey);

        
                    if (allocationMatrix.Age_Adjustment_Rules.TryGetValue(ageRuleKey, out var ageAdjustments))
                    {
                        _logger.LogInformation(ResponseMessages.AgeAdjustmentsFound, JsonSerializer.Serialize(ageAdjustments));
                        foreach (var adjustment in ageAdjustments)
                        {
                            if (finalAllocation.ContainsKey(adjustment.Key))
                            {
                                finalAllocation[adjustment.Key] += adjustment.Value;
                            }
                        }
                    }
                    else
                    {
                         _logger.LogWarning(ResponseMessages.AgeAdjustmentNotFound, ageRuleKey);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ResponseMessages.AgeAdjustmentError, ex.Message);
                    throw; 
                }
    
                // Step 4: Apply goal tuning
                try{
                    _logger.LogInformation(ResponseMessages.GoalTuningLookup, input.Goal);
                    if (string.IsNullOrEmpty(input.Goal))
                    {
                        throw new ArgumentException(ResponseMessages.GoalMissing);
                    }
                    if (allocationMatrix.Goal_Tuning.TryGetValue(input.Goal, out var goalTuning))
                    {
                        _logger.LogInformation(ResponseMessages.GoalTuningFound, JsonSerializer.Serialize(goalTuning));
        
                        switch (input.Goal)
                        {
                            case AppConstants.Goals.EmergencyFund:
                                if (finalAllocation.ContainsKey(AppConstants.AssetKeys.Cash) && finalAllocation[AppConstants.AssetKeys.Cash] < AppConstants.Thresholds.EmergencyFundCashMinimum)
                                {
                                    var cashDeficit = AppConstants.Thresholds.EmergencyFundCashMinimum - finalAllocation[AppConstants.AssetKeys.Cash];
                                    var totalReduction = 0.0;
                            
                                    // Calculate total reduction and distribute reduction proportionally in one loop
                                    foreach (var category in new[] 
                                    { 
                                        AppConstants.AssetKeys.Equity,
                                        AppConstants.AssetKeys.FixedIncome,
                                        AppConstants.AssetKeys.Commodities,
                                        AppConstants.AssetKeys.RealEstate
                                    })
                                    {
                                        if (finalAllocation.ContainsKey(category) && finalAllocation[category] > 0)
                                        {
                                            var availableReduction = Math.Max(0, finalAllocation[category]);
                                            var reduction = Math.Min(availableReduction, (cashDeficit / AppConstants.Thresholds.EmergencyFundCashMinimum) * availableReduction);
                                            finalAllocation[category] -= reduction;
                                            totalReduction += reduction;
                                        }
                                    }                            
                                    // Adjust cash allocation based on the actual reduction achieved
                                    finalAllocation[AppConstants.AssetKeys.Cash] += totalReduction;
                                }
                                break;

                            case AppConstants.Goals.Retirement:
                                if (goalTuning.TryGetValue(AppConstants.GoalTuningKeys.FixedIncomeBoost, out var fixedIncomeBoost) && finalAllocation.ContainsKey(AppConstants.AssetKeys.FixedIncome))
                                {
                                    // Ensure no negative values for fixedIncome
                                    finalAllocation[AppConstants.AssetKeys.FixedIncome] = Math.Max(0, finalAllocation[AppConstants.AssetKeys.FixedIncome] + GetDoubleFromObject(fixedIncomeBoost));
                                }
                                if (goalTuning.TryGetValue(AppConstants.GoalTuningKeys.RealEstateBoost, out var realEstateBoost) && finalAllocation.ContainsKey(AppConstants.AssetKeys.RealEstate))
                                {
                                    // Ensure no negative values for realEstate
                                    finalAllocation[AppConstants.AssetKeys.RealEstate] = Math.Max(0, finalAllocation[AppConstants.AssetKeys.RealEstate] + GetDoubleFromObject(realEstateBoost));
                                }
                                break;

                            case AppConstants.Goals.WealthAccumulation:
                                if (finalAllocation.ContainsKey(AppConstants.AssetKeys.Equity) && finalAllocation.Values.Any() && finalAllocation[AppConstants.AssetKeys.Equity] < finalAllocation.Values.Max())
                                {
                                    finalAllocation[AppConstants.AssetKeys.Equity] += AppConstants.Adjustments.EquityBoost;
                            
                                    var sumAfterEquityBoost = finalAllocation.Values.Sum();
                                    var remainingAdjustment = AppConstants.Thresholds.TotalAllocation - sumAfterEquityBoost;
                            
                                    var otherKeys = finalAllocation.Keys.Where(k => k != AppConstants.AssetKeys.Equity).ToList();
                                    if (otherKeys.Any())
                                    {
                                        foreach (var key in otherKeys)
                                        {
                                            if (finalAllocation.ContainsKey(key))
                                            {
                                                // Ensure no negative values
                                                finalAllocation[key] = Math.Max(0, finalAllocation[key] + (remainingAdjustment / otherKeys.Count()));
                                            }
                                        }
                                    }
                            
                                    // Recalculate the total and normalize if necessary
                                    var totalAfterAdjustment = finalAllocation.Values.Sum();
                                    if (Math.Abs(totalAfterAdjustment - AppConstants.Thresholds.TotalAllocation) > AppConstants.Thresholds.Tolerance)
                                    {
                                        var keyToAdjust = finalAllocation.OrderByDescending(kv => kv.Value).First().Key;
                                        finalAllocation[keyToAdjust] += AppConstants.Thresholds.TotalAllocation - totalAfterAdjustment;
                                    }
                                }
                                break;
        
                            case AppConstants.Goals.ChildEducation:
                                if (goalTuning.TryGetValue(AppConstants.GoalTuningKeys.FixedIncomeBoost, out var fixedIncomeBoostChild) && finalAllocation.ContainsKey(AppConstants.AssetKeys.FixedIncome))
                                {
                                    finalAllocation[AppConstants.AssetKeys.FixedIncome] += GetDoubleFromObject(fixedIncomeBoostChild);
                                }
                                if (goalTuning.TryGetValue(AppConstants.GoalTuningKeys.EquityReductionModerate, out var equityReduction) && finalAllocation.ContainsKey(AppConstants.AssetKeys.Equity))
                                {
                                    var reductionAmount = GetDoubleFromObject(equityReduction);
                                    finalAllocation[AppConstants.AssetKeys.Equity] = Math.Max(0, finalAllocation[AppConstants.AssetKeys.Equity] - reductionAmount);
                                }
                                break;
        
                            case AppConstants.Goals.BigPurchase:
                                if (goalTuning.TryGetValue(AppConstants.GoalTuningKeys.Balanced, out var balancedObj) &&
                                    bool.TryParse(balancedObj.ToString(), out var balanced) && balanced)
                                {
                                    _logger.LogInformation(ResponseMessages.GoalTuning.BigPurchaseBalancingEnabled);
        
                                    var threshold = AppConstants.Thresholds.BigPurchaseCapPercentage;
                                    var keys = finalAllocation.Keys.ToList();
                                    double totalExcess = 0.0;
        
                                    foreach (var assetKey in keys)
                                    {
                                        if (finalAllocation[assetKey] > threshold)
                                        {
                                            double excess = finalAllocation[assetKey] - threshold;
                                            totalExcess += excess;
                                            finalAllocation[assetKey] = threshold;
                                            _logger.LogInformation(ResponseMessages.GoalTuning.BigPurchaseCapLog, assetKey, threshold, excess);
                                        }
                                    }
        
                                    var underThresholdKeys = keys.Where(k => finalAllocation[k] < threshold).ToList();
                                    int count = underThresholdKeys.Count();
        
                                    if (count > 0 && totalExcess > 0)
                                    {
                                        double share = totalExcess / count;
                                        foreach (var key in underThresholdKeys)
                                        {
                                            finalAllocation[key] += share;
                                            _logger.LogInformation(ResponseMessages.GoalTuning.BigPurchaseShareLog, share, key, finalAllocation[key]);
                                        }
                                    }
        
                                    // Normalize after potential balancing
                                    var totalAfterBigPurchase = finalAllocation.Values.Sum();
                                    if (Math.Abs(totalAfterBigPurchase - 100) > 0.01)
                                    {
                                        var keyToAdjust = finalAllocation.OrderByDescending(kv => kv.Value).First().Key;
                                        finalAllocation[keyToAdjust] += 100 - totalAfterBigPurchase;
                                    }
                                }
                                break;
                        }
        
                        // Normalize after goal tuning adjustments
                        var totalAfterGoalTuning = finalAllocation.Values.Sum();
                        if (Math.Abs(totalAfterGoalTuning - AppConstants.Thresholds.TotalAllocation) > AppConstants.Thresholds.Tolerance)
                        {
                            var keyToAdjust = finalAllocation.OrderByDescending(kv => kv.Value).First().Key;
                            finalAllocation[keyToAdjust] += AppConstants.Thresholds.TotalAllocation - totalAfterGoalTuning;
                        }
                    }
                    else
                    {
                        _logger.LogWarning(ResponseMessages.GoalNotFound, input.Goal);
                    }
                }

                catch (Exception ex)
                {
                    _logger.LogError(ex, ResponseMessages.GoalTuningError, ex.Message);
                    throw; 
                }
    
            try
            {
                var total = finalAllocation.Values.Sum();
                foreach (var key in finalAllocation.Keys.ToList())
                {
                    finalAllocation[key] = Math.Max(0, finalAllocation[key]);
                }

                // Recalculate total after removing negative values
                total = finalAllocation.Values.Sum();
                if (Math.Abs(total - AppConstants.Thresholds.TotalAllocation) > AppConstants.Thresholds.Tolerance)
                {
                    var adjustmentFactor = AppConstants.Thresholds.TotalAllocation / total;
    
                    foreach (var key in finalAllocation.Keys.ToList())
                    {
                        finalAllocation[key] = Math.Max(0, finalAllocation[key] * adjustmentFactor);
                    }
    
                    // Recalculate the total after adjustment
                    var adjustedTotal = finalAllocation.Values.Sum();
    
                    // If the total is still not 100%, adjust the largest allocation to make up the difference
                    if (Math.Abs(adjustedTotal - AppConstants.Thresholds.TotalAllocation) > AppConstants.Thresholds.Tolerance)
                    {
                        var keyToAdjust = finalAllocation.OrderByDescending(kv => kv.Value).First().Key;
                        finalAllocation[keyToAdjust] = Math.Max(0, finalAllocation[keyToAdjust] + AppConstants.Thresholds.TotalAllocation - adjustedTotal);
                    }
                }
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, ResponseMessages.NormalizationError);
                throw; // Re-throw the exception to propagate it to the caller
            }

                // Step 6: Compute and add sub-allocations
                try
                {
                    var subMatrix = await LoadSubAllocationMatrixAsync();
                    var subAllocations = ComputeSubAllocations(finalAllocation, input.RiskTolerance!, subMatrix);
        
                    // Step 7: Format the final result according to the expected structure
                    var finalFormattedResult = new Dictionary<string, object>();
        
                    foreach (var mainAllocationPair in finalAllocation)
                    {
                        var assetClassName = mainAllocationPair.Key;
                        var assetPercentage = mainAllocationPair.Value;
        
                        if (subAllocations.TryGetValue(assetClassName, out var subAssetAllocations))
                        {
                            finalFormattedResult[assetClassName] = new Dictionary<string, object>
                            {
                                [AppConstants.Percentage] = Math.Round(assetPercentage, 2),
                                [AppConstants.subAssets] = subAssetAllocations
                            };
                           _logger.LogInformation(ResponseMessages.SubAssetsAdded, assetClassName, JsonSerializer.Serialize(subAllocations[assetClassName]));
                        }
                        else
                        {
                            finalFormattedResult[assetClassName] = new Dictionary<string, object>
                            {
                                [AppConstants.Percentage] = Math.Round(assetPercentage, 2),
                                [AppConstants.subAssets] = new Dictionary<string, double>() // Empty if no sub-allocations
                            };
                            _logger.LogWarning(ResponseMessages.NoSubAssetsFound, assetClassName);
                        }
                    }
                    _logger.LogInformation(ResponseMessages.FinalFormattedResult, JsonSerializer.Serialize(finalFormattedResult));

                    var result = new Dictionary<string, object> { [AppConstants.Assets] = finalFormattedResult };

                    // Save the result to a JSON file
                    await SaveFinalAllocationToFileAsync(result);
                    _logger.LogInformation(ResponseMessages.FinalFormattedResult, JsonSerializer.Serialize(finalFormattedResult));                     
                    return result;
                }

                catch (Exception ex)
                {
                     _logger.LogError(ex,ResponseMessages.SubAllocationComputationError);
                    throw; // Re-throw the exception to propagate it to the caller
                }
            }

            catch(Exception ex)
            {
                _logger.LogError(ex, ResponseMessages.AllocationCalculationError);
                throw; // Re-throw the exception to propagate it to the caller
            }
        }
         
        private string GetAgeGroup(int age)
        {
            if (age < 30) return AppConstants.AgeGroup.UnderThirty;
            if (age <= 45) return AppConstants.AgeGroup.ThirtyToFortyFive;
            if (age <= 60) return AppConstants.AgeGroup.FortyFiveToSixty;
            return AppConstants.AgeGroup.AboveSixty;
        }

        private string GetHorizonGroup(int investmentHorizon)
        {
            if (investmentHorizon < 0)
                throw new ArgumentException(ResponseMessages.InvalidHorizon);
        
            if (investmentHorizon < 4) return AppConstants.horizonGroup.HorizonShort;
            if (investmentHorizon < 7) return AppConstants.horizonGroup.HorizonModerate;
            return AppConstants.horizonGroup.HorizonLong;
        }
 
        private double GetDoubleFromObject(object obj)
        {
            if (obj is JsonElement jsonElement)
            {
                if (jsonElement.ValueKind == JsonValueKind.Number)
                {
                    return jsonElement.GetDouble();
                }
                throw new InvalidCastException(string.Format(ResponseMessages.JsonElementNotNumber, jsonElement.ValueKind, jsonElement));
            }
 
            if (obj is IConvertible convertible)
            {
                return convertible.ToDouble(null);
            }
            throw new InvalidCastException(string.Format(ResponseMessages.ObjectConversionError, obj.GetType(), obj));
        }

        public async Task<Dictionary<string, object>> GetFinalAllocationFromFileAsync()
        {
            try
            {
                var filePath = AppConstants.FinalAllocationFilePath; // Path to the saved JSON file
                
                _logger.LogInformation(ResponseMessages.ReadingFinalAllocation, filePath);
                
                        if (!File.Exists(filePath))
                        {
                            _logger.LogWarning(ResponseMessages.FinalAllocationFileNotFound, filePath);
                            return null;
                        }
                
                        var json = await File.ReadAllTextAsync(filePath);
                        _logger.LogInformation(ResponseMessages.FinalAllocationReadSuccess);
                
                        var finalAllocation = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                
                        if (finalAllocation == null)
                        {
                            _logger.LogWarning(ResponseMessages.FinalAllocationNull);
                            return null;
                        }
                
                        return finalAllocation;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ResponseMessages.FinalAllocationReadError);
                        throw;
                    }
                }
 
        private async Task<SubAllocationMatrix> LoadSubAllocationMatrixAsync()
        {
            try{
            var filePath = AppConstants.SubAllocationMatrix;

            if (!File.Exists(filePath))
                throw new FileNotFoundException(ResponseMessages.SubAllocationMatrixNotFound, filePath);

            var json = await File.ReadAllTextAsync(filePath);
            var intMatrix = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, int>>>>(json);

            var doubleMatrix = intMatrix.ToDictionary(
                outer => outer.Key,
                outer => outer.Value.ToDictionary(
                    middle => middle.Key,
                    middle => middle.Value.ToDictionary(
                        inner => inner.Key,
                        inner => (double)inner.Value
                    )
                )
            );

            return new SubAllocationMatrix { Matrix = doubleMatrix };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,ResponseMessages.SubAllocationMatrixLoadError);
                throw; // Re-throw the exception to propagate it to the caller
            }
        }

        
        private Dictionary<string, Dictionary<string, double>> ComputeSubAllocations(
    Dictionary<string, double> finalAllocations,
    string riskLevel,
    SubAllocationMatrix subMatrix)
    {
        try{
        var subAllocationsResult = new Dictionary<string, Dictionary<string, double>>();
        // var assetClassMapping = new Dictionary<string, string>
        // {
        //     { "equity", "Equity" },
        //     { "fixedIncome", "Fixed Income" },
        //     { "commodities", "Commodities" },
        //     { "cash", "Cash Equivalence" },
        //     { "realEstate", "Real Estate" }
        // };

        foreach (var assetClass in finalAllocations)
        {
            var className = assetClass.Key;
            var totalPercentage = assetClass.Value;

            if (!AppConstants.AssetClassMappings.TryGetValue(className, out var mappedClassName))
            {
                _logger.LogWarning(ResponseMessages.NoMappingFoundForAssetClass, className);
                continue;
            }

            if (!subMatrix.Matrix.ContainsKey(mappedClassName))
            {
                _logger.LogWarning(ResponseMessages.NoSubAllocationRulesFound, mappedClassName);
                continue; // Skip if no suballocation rules for this asset class
            }

            var subcategories = subMatrix.Matrix[mappedClassName];
            var weights = new Dictionary<string, int>();

            // Collect weights for this risk level
            foreach (var sub in subcategories)
            {
                if (sub.Value.ContainsKey(riskLevel))
                {
                    weights[sub.Key] = (int)sub.Value[riskLevel];
                }
            }

            var totalWeight = weights.Values.Sum();
            if (totalWeight == 0)
            {
                _logger.LogWarning(ResponseMessages.NoWeightsForRiskLevel, riskLevel, className);
                continue;
            }

            // Calculate suballocation % based on weight
            var calculatedSubs = weights.ToDictionary(
                kv => kv.Key,
                // kv => Math.Round((kv.Value / (double)totalWeight) * totalPercentage, 2)
                kv =>
    {
        var subAllocation = Math.Round((kv.Value / (double)totalWeight) * totalPercentage, 2);
        return Math.Max(0, subAllocation); // Ensure no negative sub-allocations
    }
            );
            _logger.LogInformation(ResponseMessages.ComputedSubAllocations, className, JsonSerializer.Serialize(calculatedSubs));
            subAllocationsResult[className] = calculatedSubs;
        }

        return subAllocationsResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ResponseMessages.SubAllocationComputationError);
            throw; // Re-throw the exception to propagate it to the caller
        }
}

public Dictionary<string, Dictionary<string, double>> TransformAssetsToSubAllocationResult(dynamic assets)
{
    var subAllocationResult = new Dictionary<string, Dictionary<string, double>>();

    foreach (var assetClass in assets)
    {
        var assetClassName = assetClass.Name; // e.g., "equity", "fixedIncome"
        var subAssets = assetClass.Value.subAssets;

        var subAssetAllocations = new Dictionary<string, double>();
        foreach (var subAsset in subAssets)
        {
            var subAssetName = subAsset.Name; // e.g., "Large Cap", "Gov Bonds"
            var allocation = (double)subAsset.Value; // Allocation percentage
            subAssetAllocations[subAssetName] = allocation;
        }

        subAllocationResult[assetClassName] = subAssetAllocations;
    }

    return subAllocationResult;
}
    }
}
