﻿using System.ComponentModel.DataAnnotations.Schema;

namespace MojProjekt.Models
{
    public class SerialNumber
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int? ItemId { get; set; }
        [ForeignKey("ItemId")]
        public Item? Item { get; set; }
    }
}
