﻿using LaptopService.Core.Services.Interface;
using LaptopService.Dtos;
using LaptopService.Models;
using LaptopService.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;

namespace LaptopWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly ILaptopService _laptopService;
        private readonly EncryptionLogic _encryption;

        public DeviceController(ILaptopService laptopService)
        {
            try
            {
                _laptopService = laptopService;
            }
            catch (Exception ex)
            {
                StatusCode(500, "An Error has occured:" + ex.Message);
                throw;
            }
        }

        [HttpGet]
        [Route("GetAllLaptopDetails")]

        public IActionResult GetAllLaptopDetails()
        {
            try
            {
                var laptops = _laptopService.GetAllLaptopDetails();
                if (laptops == null || !laptops.Any())
                    return NotFound("No Laptops are there");

                var laptopDtos = laptops.Select(LaptopMapper.ToDto);
                return Ok(laptopDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An Error has occured:" + ex.Message);
            }
        }

        [HttpGet]
        [Route("GetLaptopDetailsById/{id}")]
        public ActionResult<LaptopDto> GetLaptopDetailsById(int id)
        {
            try
            {
                var laptop = _laptopService.GetLaptopDetailsById(id);
                if (laptop == null)
                    return NotFound($"Laptop with ID {id} does not exist.");

                return Ok(LaptopMapper.ToDto(laptop));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An Error has occured:" + ex.Message);
            }
        }

        [HttpPost]
        [Route("AddNewLaptopDetails")]
        public IActionResult AddNewLaptopDetails(LaptopDto laptopDto)
        {
            try
            {
                var laptop = LaptopMapper.ToEntity(laptopDto);
                _laptopService.AddNewLaptopDetails(laptop);
                return Ok("Laptop added successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An Error has occured:" + ex.Message);
            }
        }


        [HttpPost]
        [Route("UpdateLaptopDetails")]
        public IActionResult UpdateLaptopDetails(LaptopDto laptopDto)
        {
            try
            {
                var existingLaptop = _laptopService.GetLaptopDetailsById(laptopDto.Id);
                if (existingLaptop == null)
                    return NotFound($"Laptop with ID {laptopDto.Id} does not exist.");

                var laptop = LaptopMapper.ToEntity(laptopDto);
                _laptopService.UpdateLaptopDetails(laptop);
                return Ok("Laptop updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An Error has occured:" + ex.Message);
            }
        }

        [HttpPost("{id}")]

        public IActionResult Delete(int id)
        {
            try
            {
                var existingLaptop = _laptopService.GetLaptopDetailsById(id);
                if (existingLaptop == null)
                {
                    return NotFound($"Laptop with ID {id} does not exist.");
                }

                _laptopService.DeleteLaptopById(id);
                return Ok($"Laptop with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An Error has occured:" + ex.Message);
            }
        }
        [HttpGet]
        [Route("GetLaptopByAssetTag/{assetTag}")]
        public IActionResult GetLaptopByAssetTag(int assetTag)
        {
            try
            {
                var laptop = _laptopService.GetLaptopByAssetTag(assetTag);
                if (laptop == null)
                    return NotFound($"Laptop with AssetTag {assetTag} does not exist.");

                var laptopDto = LaptopMapper.ToDto(laptop);
                return Ok(laptopDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An Error has occured:" + ex.Message);
            }
        }


        [HttpPost("AddComment")]
        public IActionResult AddComment([FromBody] AssetComment comment)
        {
            try
            {
                if (comment == null || string.IsNullOrWhiteSpace(comment.Commentor) || string.IsNullOrWhiteSpace(comment.Comment))
                    return BadRequest("Invalid comment data.");

                comment.Date = comment.Date == default ? DateTime.UtcNow : comment.Date;
                _laptopService.AddComment(comment);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        [HttpGet("GetComments/{assetId}")]
        public IActionResult GetComments(int assetId)
        {
            try
            {

                var comments = _laptopService.GetComments(assetId);
                var result = comments.Select(c => new { c.Date, c.Commentor, c.Comment });
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }




    }
}
