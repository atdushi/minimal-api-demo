using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MagicVilla_CouponAPI.Endpoints
{
    public static class CouponEndpoints
    {
        public static void ConfigureCouponEndpoints(this WebApplication app)
        {
            app.MapGet("/api/coupon", GetAllCoupons)
                .WithName("GetCoupons")
                .Produces<APIResponse>(200);

            app.MapGet("/api/coupon/{id:int}", GetCoupon)
                .WithName("GetCoupon")
                .Produces<APIResponse>(200);

            app.MapPost("/api/coupon", CreateCoupon)
                .WithName("CreateCoupon")
                .Accepts<CouponCreateDTO>("application/json")
                .Produces<APIResponse>(201)
                .Produces(400);

            app.MapPut("/api/coupon", UpdateCoupon)
                .WithName("UpdateCoupon")
                .Produces<APIResponse>(200)
                .Produces(400);

            app.MapDelete("/api/coupon/{id:int}", DeleteCoupon)
                .WithName("DeleteCoupon")
                .Produces<APIResponse>(200);
        }

        private static async Task<IResult> GetCoupon(ICouponRepository couponRepository, ILogger<Program> logger, int id)
        {
            APIResponse response = new();
            response.Result = await couponRepository.GetAsync(id);
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }

        private static async Task<IResult> CreateCoupon(ICouponRepository couponRepository,
                IMapper mapper, IValidator<CouponCreateDTO> validation, [FromBody] CouponCreateDTO coupon_C_DTO)
        {
            APIResponse response = new();

            var validationResult = await validation.ValidateAsync(coupon_C_DTO);

            if (!validationResult.IsValid)
            {
                response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
                response.StatusCode = HttpStatusCode.BadRequest;
                return Results.BadRequest(response);
            }
            if (await couponRepository.GetAsync(coupon_C_DTO.Name) != null)
            {
                response.ErrorMessages.Add("Coupon Name already exists");
                response.StatusCode = HttpStatusCode.BadRequest;
                return Results.BadRequest(response);
            }

            Coupon coupon = mapper.Map<Coupon>(coupon_C_DTO);
            await couponRepository.CreateAsync(coupon);
            await couponRepository.SaveAsync();

            CouponDTO couponDTO = mapper.Map<CouponDTO>(coupon);
            response.Result = couponDTO;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.Created;
            return Results.Created($"/api/coupon/{coupon.Id}", response);

            //return Results.CreatedAtRoute("GetCoupon", new { id = couponDTO.Id }, couponDTO);
            //return Results.Created($"/api/coupon/{coupon.Id}", coupon);
        }

        private static async Task<IResult> UpdateCoupon(ICouponRepository couponRepository,
                IMapper mapper, IValidator<CouponUpdateDTO> validation, [FromBody] CouponUpdateDTO coupon_U_DTO)
        {
            APIResponse response = new();

            var validationResult = await validation.ValidateAsync(coupon_U_DTO);

            if (!validationResult.IsValid)
            {
                response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
                response.StatusCode = HttpStatusCode.BadRequest;
                return Results.BadRequest(response);
            }

            await couponRepository.UpdateAsync(mapper.Map<Coupon>(coupon_U_DTO));
            await couponRepository.SaveAsync();

            response.Result = mapper.Map<CouponDTO>(await couponRepository.GetAsync(coupon_U_DTO.Id));
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }

        private static async Task<IResult> DeleteCoupon(ICouponRepository couponRepository, int id)
        {
            APIResponse response = new();
            Coupon coupon = await couponRepository.GetAsync(id);
            if (coupon == null)
            {
                response.ErrorMessages.Add("Coupon with id = " + id + " not found!");
                response.StatusCode = HttpStatusCode.BadRequest;
                return Results.BadRequest(response);
            }

            await couponRepository.RemoveAsync(coupon);
            await couponRepository.SaveAsync();

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.NoContent;
            return Results.Ok(response);
        }

        private static async Task<IResult> GetAllCoupons(ICouponRepository couponRepository, ILogger<Coupon> logger)
        {
            logger.Log(LogLevel.Information, "Get all coupons.");

            APIResponse response = new()
            {
                IsSuccess = true,
                Result = await couponRepository.GetAllAsync(),
                StatusCode = HttpStatusCode.OK
            };

            return Results.Ok(response);
        }
    }
}
