namespace WisVestAPI.Constants
{
    public static class AppConstants
    {
        // Age limits
        public const int MinAge = 18;
        public const int MaxAge = 100;

        // Investment horizon limits (years)
        public const int MinInvestmentHorizon = 1;
        public const int MaxInvestmentHorizon = 30;

        // Target amount limits (INR)
        public const int MinTargetAmount = 10_000;
        public const int MaxTargetAmount = 100_000_000;

        // Allowed risk tolerances
        public static readonly string[] ValidRiskTolerances = { "Low", "Medium", "High" };

        public const string Percentage= "percentage";
        public const string subAssets = "subAssets";
        public const string Assets = "assets";

        //AllocationService
        public const string FinalAllocationFilePath = "FinalAllocation.json";
        public const string SubAllocationMatrix = "Repositories/Matrix/SubAllocationMatrix.json";

        public const string RiskLow = "Low";
        public const string RiskMedium = "Medium";
        public const string RiskHigh = "High";
        public const string RiskMidMapped = "Mid";

        public const string HorizonShort = "short";
        public const string HorizonModerate = "moderate";
        public const string HorizonLong = "long";

        public const string HorizonShortMapped = "Short";
        public const string HorizonModerateMapped = "Mod";
        public const string HorizonLongMapped = "Long";
        public static class Goals
    {
        public const string EmergencyFund = "Emergency Fund";
        public const string Retirement = "Retirement";
        public const string WealthAccumulation = "Wealth Accumulation";
        public const string ChildEducation = "Child Education";
        public const string BigPurchase = "Big Purchase";
    }

    public static class AssetKeys
    {
        public const string Cash = "cash";
        public const string Equity = "equity";
        public const string FixedIncome = "fixedIncome";
        public const string Commodities = "commodities";
        public const string RealEstate = "realEstate";
    }

    public static class GoalTuningKeys
    {
        public const string FixedIncomeBoost = "fixedIncome_boost";
        public const string EquityReductionModerate = "equityReduction_moderate";
        // public const string FixedIncomeBoost = "FixedIncomeBoost";
        public const string RealEstateBoost = "realEstate_boost";
        // public const string EquityReductionModerate = "EquityReductionModerate";
        public const string Balanced = "balanced";
    }

    public static class Thresholds
    {
        public const double EmergencyFundCashMinimum = 40;
        public const double TotalAllocation = 100;
        public const double BigPurchaseCapPercentage = 30.0;
        public const double TotalAllocation_c= 100.0;
        public const double Tolerance = 0.01;
    }

    public static class AgeGroup
    {
        public const string UnderThirty = "<30";
        public const string ThirtyToFortyFive = "30-45";
        public const string FortyFiveToSixty = "45-60";
        public const string AboveSixty = "60+";
    }

        public static class horizonGroup
    {
        public const string HorizonShort = "short";
        public const string HorizonModerate = "moderate";
        public const string HorizonLong = "long";
    }

    public static class Adjustments
    {
        public const double EquityBoost = 10;
    }

    public static readonly Dictionary<string, string> AssetClassMappings = new Dictionary<string, string>
    {
        { AssetKeys.Equity, "Equity" },
        { AssetKeys.FixedIncome, "Fixed Income" },
        { AssetKeys.Commodities, "Commodities" },
        { AssetKeys.Cash, "Cash Equivalence" },
        { AssetKeys.RealEstate, "Real Estate" }
    };
    
    }
}