
namespace RTS.Commands
{
    public interface ICommand
    {
        public bool IsSingleUnitCommand { get; }
        bool CanHandle(CommandContext ctx);
        void Handle(CommandContext ctx);
    }
}