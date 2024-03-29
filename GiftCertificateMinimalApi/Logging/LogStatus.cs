﻿using System.Runtime.Serialization;

namespace GiftCertificateMinimalApi.Logging
{
    public enum LogStatus
    {
        [EnumMember(Value = "Info")]
        Info,
        [EnumMember(Value = "Error")]
        Error,
        [EnumMember(Value = "Ok")]
        Ok
    }
}
