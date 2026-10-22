using System.ComponentModel;
using System.Runtime.CompilerServices;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow
{
    public sealed class MultiRoutePaymentDraftRow : INotifyPropertyChanged
    {
        private readonly decimal[] _amounts = new decimal[11];
        private bool _minimumWageSupportEnabled = true;
        private decimal _minimumWageSupport;
        private TransportationProgramType _program = TransportationProgramType.SpecialEducation;
        private decimal _contractAmount;
        private bool _vatExempt;
        private ProgressPaymentCalculationConstants _constants = ProgressPaymentCalculationConstants.Defaults;
        public event PropertyChangedEventHandler? PropertyChanged;
        public string RouteKey { get; set; } = string.Empty;
        public string RouteNumber { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public string RouteDisplayText => $"Hat {RouteNumber} — {RouteName}";
        public decimal WorkDays { get => _amounts[0]; set { if (_amounts[0] == value) return; _amounts[0] = value; Changed(); } }
        public decimal DailyPrice { get => _amounts[1]; set { if (_amounts[1] == value) return; _amounts[1] = value; Changed(); } }
        public decimal DiscountAmount { get => _amounts[2]; set { if (_amounts[2] == value) return; _amounts[2] = value; Changed(); } }
        public decimal PriceDifference { get => _amounts[3]; set { if (_amounts[3] == value) return; _amounts[3] = value; Changed(); } }
        public decimal GuideWorkDays { get => _amounts[4]; set { if (_amounts[4] == value) return; _amounts[4] = value; Changed(); } }
        public decimal GuideDailyPrice { get => _amounts[5]; set { if (_amounts[5] == value) return; _amounts[5] = value; Changed(); } }
        public decimal GuideDiscountAmount { get => _amounts[6]; set { if (_amounts[6] == value) return; _amounts[6] = value; Changed(); } }
        public decimal GuidePriceDifference { get => _amounts[7]; set { if (_amounts[7] == value) return; _amounts[7] = value; Changed(); } }
        public decimal TaxDebt { get => _amounts[8]; set { if (_amounts[8] == value) return; _amounts[8] = value; Changed(); } }
        public decimal SocialSecurityDebt { get => _amounts[9]; set { if (_amounts[9] == value) return; _amounts[9] = value; Changed(); } }
        public decimal InspectionPenalty { get => _amounts[10]; set { if (_amounts[10] == value) return; _amounts[10] = value; Changed(); } }
        public bool MinimumWageSupportEnabled
        {
            get => _minimumWageSupportEnabled;
            set
            {
                if (_minimumWageSupportEnabled == value) return;
                _minimumWageSupportEnabled = value;
                if (!value) _minimumWageSupport = 0m;
                Changed(); PropertyChanged?.Invoke(this, new(nameof(MinimumWageSupport)));
            }
        }
        public decimal MinimumWageSupport
        {
            get => _minimumWageSupport;
            set { var amount = MinimumWageSupportEnabled ? value : 0m; if (_minimumWageSupport == amount) return; _minimumWageSupport = amount; Changed(); }
        }
        public void Configure(TransportationProgramType program, decimal contractAmount, bool vatExempt, ProgressPaymentCalculationConstants constants)
        {
            _program=program; _contractAmount=contractAmount; _vatExempt=vatExempt; _constants=constants;
            Changed(string.Empty);
        }
        public ProgressPaymentInput CreateInput() => new(WorkDays,DailyPrice,DiscountAmount,PriceDifference,
            _contractAmount,_vatExempt,TaxDebt,SocialSecurityDebt,MinimumWageSupportEnabled?MinimumWageSupport:0m,
            InspectionPenalty,_program==TransportationProgramType.SpecialEducation?GuideWorkDays:0m,
            _program==TransportationProgramType.SpecialEducation?GuideDailyPrice:0m,
            _program==TransportationProgramType.SpecialEducation?GuideDiscountAmount:0m,
            _program==TransportationProgramType.SpecialEducation?GuidePriceDifference:0m,_program);
        public ProgressPaymentCalculation? Preview
        {
            get
            {
                try { return ProgressPaymentCalculator.Calculate(CreateInput(),_constants); }
                catch(Exception ex) when(ex is ArgumentException or OverflowException) { return null; }
            }
        }
        public string NetPayableText => Preview is {} result ? result.NetPayable.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("tr-TR"))+" ₺" : "Girdiyi kontrol edin";
        private void Changed([CallerMemberName] string? property=null)
        {
            PropertyChanged?.Invoke(this,new(property));
            PropertyChanged?.Invoke(this,new(nameof(NetPayableText)));
        }
    }
}
