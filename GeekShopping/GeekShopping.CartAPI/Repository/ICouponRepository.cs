using System.Threading.Tasks;
using GeekShopping.CartAPI.Data.ValueObjects;

namespace GeekShopping.CartAPI.Repository;

public interface ICouponRepository
{
    Task<CouponVO> GetCoupon(string couponCode, string token);
}