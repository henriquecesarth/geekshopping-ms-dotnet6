using System.ComponentModel.DataAnnotations.Schema;
using GeekShopping.CartAPI.Model.Base;

namespace GeekShopping.CartAPI.Model;

[Table("cart_detail")]
public class CartDetail : BaseEntity
{
    public long CartHeaderId { get; set; }

    [ForeignKey("CartHeaderId")]
    public CartHeader CartHeader { get; set; }

    public long ProductId { get; set; }
    
    [ForeignKey("ProductId")] 
    public Product Product { get; set; }

    [Column("count")]
    public int Count { get; set; }
}