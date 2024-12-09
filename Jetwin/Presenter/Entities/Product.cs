namespace Jetwin.Presenter.Entities
{
    public class Product
    {
        public string ProductName { get; set; }
        public int CategoryID { get; set; }
        public int BrandID { get; set; }
        public int SupplierID { get; set; }
        public decimal UnitPrice { get; set; }
        public int UoMID { get; set; }
        public int ProductCode { get; internal set; }
    }
}
