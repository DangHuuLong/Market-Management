using PBL3_HK4.Entity;

namespace PBL3_HK4.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Catalog> Catalogs { get; set; }

        public List<ProductImage> ProductImages { get; set; }

        public void AssignImagesToProducts()
        {
            foreach (var product in Products)
            {
                var images = ProductImages
                    .Where(image => image.ProductID == product.ProductID)
                    .ToList();
                product.Images = images;
            }
        }
    }
}