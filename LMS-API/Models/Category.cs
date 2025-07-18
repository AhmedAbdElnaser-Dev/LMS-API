﻿using System.ComponentModel.DataAnnotations;

namespace LMS_API.Models
{
    public class Category: BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Name { get; set; }
    }
}
