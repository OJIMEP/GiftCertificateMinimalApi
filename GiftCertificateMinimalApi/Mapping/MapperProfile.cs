﻿using AutoMapper;
using GiftCertificateMinimalApi.Models;
using System.Data;
using System.Data.Common;

namespace GiftCertificateMinimalApi.Mapping
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<DbDataReader, CertGetResponseDto>()
                .ForMember(dest => dest.Barcode, opt => opt.MapFrom(src => src.GetString("Barcode")))
                .ForMember(dest => dest.Sum, opt => opt.MapFrom(src => src.GetDecimal("SumLeft")))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.GetInt32("IsActive")))
                .ForMember(dest => dest.IsValid, opt => opt.MapFrom(src => src.GetInt32("IsValid")));
        }
    }
}
