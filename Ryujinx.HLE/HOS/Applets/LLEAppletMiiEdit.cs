
namespace Ryujinx.HLE.HOS.Applets
{
    class LLEAppletMiiEdit : LLEApplet
    {
        public LLEAppletMiiEdit(Horizon system) : base(system) { }

        protected override ulong GetTitleId()
        {
            return 0x0100000000001009;
        }
    }
}
