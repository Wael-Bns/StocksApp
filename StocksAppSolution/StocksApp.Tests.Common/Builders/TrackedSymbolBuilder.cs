using StocksApp.Domain.Entities;

namespace StocksApp.Tests.Common.Builders
{
    public class TrackedSymbolBuilder
    {
        private string _symbol = "AAPL";
        private string _displayName = "Apple Inc.";
        private bool _isActive = true;

        public TrackedSymbolBuilder WithSymbol(string symbol)
        {
            _symbol = symbol;
            return this;
        }

        public TrackedSymbolBuilder WithDisplayName(string displayName)
        {
            _displayName = displayName;
            return this;
        }

        public TrackedSymbolBuilder Inactive()
        {
            _isActive = false;
            return this;
        }

        // timestamps are left unset on purpose so the DB defaults apply
        public TrackedSymbol Build() => new()
        {
            Symbol = _symbol,
            DisplayName = _displayName,
            IsActive = _isActive
        };
    }
}