namespace Ryujinx.HLE.HOS.Services.Hid
{
    enum ResultCode
    {
        ModuleId       = 2,
        ErrorCodeShift = 202,

        Success = 0,

        InvalidNpadIdType = (710 << ErrorCodeShift) | ModuleId
    }
}