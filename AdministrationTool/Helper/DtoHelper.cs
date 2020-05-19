using iSIM.Core.Common.Enum;
using iSIM.Core.Entity.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSIMAssetTool.Helper
{
    public static class DtoHelper
    {


        //public static PoleDto CreatePoleDto(Pole pole)
        //{
        //    PoleDto defaultPoleDto = new PoleDto() { Id = 0 };
        //    defaultPoleDto.AssetDto = new AssetDto() { Id = 0, IsActive = pole.IsActive, Name = pole.AssetName };
        //    defaultPoleDto.AssetDto.GeometryDto = new GeometryDto()
        //    {
        //        Id = 0,
        //        Longitude = pole.Longtitude,
        //        Latitude = pole.Latitude,
        //        Altitude = -1
        //    };
        //    defaultPoleDto.AssetDto.AssetTypeDto = new AssetTypeDto()
        //    {
        //        AssetType = iSIM.Core.Common.Enum.AssetType.Pole,
        //        Id = (long)iSIM.Core.Common.Enum.AssetType.Pole,
        //        Name = iSIM.Core.Common.Enum.AssetType.Pole.ToString()
        //    };
        //    defaultPoleDto.AssetDto.AssetLocationTypeDto = new AssetLocationTypeDto()
        //    {
        //        AssetLocationType = iSIM.Core.Common.Enum.AssetLocationType.Point,
        //        Id = (long)iSIM.Core.Common.Enum.AssetLocationType.Point,
        //        Name = iSIM.Core.Common.Enum.AssetLocationType.Point.ToString()
        //    };
        //    defaultPoleDto.AssetDto.EndpointDtos = new List<EndpointDto>();
        //    return defaultPoleDto;
        //}

        //public static AssetGroupDto CreateAssetGroupDto(AssetGroup assetGroup)
        //{

        //    AssetGroupDto defaultAssetGroupDto = new AssetGroupDto();
        //    defaultAssetGroupDto.AssetDto = new AssetDto() { Id = 0, IsActive = assetGroup.IsActive, Name = assetGroup.Name };
        //    defaultAssetGroupDto.AssetDto.GeometryDto = new GeometryDto()
        //    {
        //        Id = 0,
        //        Longitude = -1,
        //        Latitude = -1,
        //        Altitude = -1
        //    };
        //    defaultAssetGroupDto.AssetDto.AssetTypeDto = new AssetTypeDto()
        //    {
        //        AssetType = iSIM.Core.Common.Enum.AssetType.AssetGroup,
        //        Id = (long)iSIM.Core.Common.Enum.AssetType.AssetGroup,
        //        Name = iSIM.Core.Common.Enum.AssetType.AssetGroup.ToString()
        //    };
        //    defaultAssetGroupDto.AssetDto.AssetLocationTypeDto = new AssetLocationTypeDto()
        //    {
        //        AssetLocationType = iSIM.Core.Common.Enum.AssetLocationType.Point,
        //        Id = (long)iSIM.Core.Common.Enum.AssetLocationType.Point,
        //        Name = iSIM.Core.Common.Enum.AssetLocationType.Point.ToString()
        //    };
        //    defaultAssetGroupDto.AssetDto.EndpointDtos = new List<EndpointDto>();

        //    return defaultAssetGroupDto;
        //}

        public static CameraDto CreateCameraDto()
        {

            CameraDto defaultCameraDto = new CameraDto();
            defaultCameraDto.AssetDto = new AssetDto() { Id = 0, IsActive = true };
            defaultCameraDto.AssetDto.GeometryDto = new GeometryDto()
            {
                Id = 0,
                Longitude = -1,
                Latitude = -1,
                Altitude = -1
            };
            defaultCameraDto.AssetDto.AssetTypeDto = new AssetTypeDto()
            {
                AssetType = AssetType.Vms,
                Id = (long)AssetType.Vms,
                Name = AssetType.Vms.ToString()
            };
            defaultCameraDto.AssetDto.AssetLocationTypeDto = new AssetLocationTypeDto()
            {
                AssetLocationType = AssetLocationType.Point,
                Id = (long)AssetLocationType.Point,
                Name = AssetLocationType.Point.ToString()
            };
            defaultCameraDto.ViewPortDto = new ViewPortDto()
            {
                Id = 0
            };
            defaultCameraDto.AssetDto.EndpointDtos = new List<EndpointDto>();
            return defaultCameraDto;
        }
    }
}
