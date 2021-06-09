﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TLOSoltuion.Data.EF;
using TLOSoltuion.Data.Entities;
using TLOSolution.WebApp.Models;

namespace TLOSolution.WebApp.Controllers
{
    public class PostController : Controller
    {
        private readonly TLODbContext _context;
        private readonly IHostingEnvironment _environment;

        public PostController(TLODbContext context, IHostingEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var result = _context.Post.Where(x => x.Id == id).FirstOrDefault();
            if (result == null)
            {
                return View();
            }

            var post = new PostDetailModel()
            {
                Title = result.Title,
                Description = result.Description,
                DocumentPath = result.DocumentPath
            };

            return View(post);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async void Create(PostCreateModel request)
        {
            if (!ModelState.IsValid)
            {
                return;
            }

            if (!await UploadFile(request.DocumentPath))
            {
                ModelState.AddModelError("", "cannot upload file");
                return;
            }

            var post = new Post()
            {
                Title = request.Title,
                Description = request.Description,
                DocumentPath = request.DocumentPath.FileName
            };

            await _context.Post.AddAsync(post);
            await _context.SaveChangesAsync();
        }

        private async Task<bool> UploadFile(IFormFile ufile)
        {
            if (ufile != null && ufile.Length > 0)
            {
                var fileName = Path.GetFileName(ufile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\content", fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ufile.CopyToAsync(fileStream);
                }
                return true;
            }
            return false;
        }
    }
}