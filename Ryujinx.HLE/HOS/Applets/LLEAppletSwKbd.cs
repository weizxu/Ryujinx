
namespace Ryujinx.HLE.HOS.Applets
{
    class LLEAppletSwKbd : LLEApplet
    {
        public LLEAppletSwKbd(Horizon system) : base(system) { }

        protected override ulong GetTitleId()
        {
            return 0x0100000000001008;
        }
    }
}
