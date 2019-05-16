using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PayPal.Api;
using Project_BookStoreCT.Models.DataModels;
using Project_BookStoreCT.Models.PostModels;
using Project_BookStoreCT.Models.ServiceModels;
using Project_BookStoreCT.Models.ViewModels;

namespace Project_BookStoreCT.Controllers
{
    public class HomeController : Controller
    {
        //Trang chủ
        public ActionResult Index()
        {
            using (DataContext db = new DataContext())
            {
                ViewBag.GetAllBooks = (from b in db.Books select b).ToList();
                ViewBag.GetAllBooksSaleOff = (from b in db.Books where b.statusSaleOff == true select b).ToList();
                ViewBag.GetAllBooksHighlights = (from b in db.Books orderby b.sellNumber descending select b).Take(6).ToList();
            }
            return View();
        }
        //Lấy dữ liệu cho partial menu sách trong nước
        public PartialViewResult _PartialMenuSachTrongNuoc()
        {
            using (DataContext db = new DataContext())
            {

                List<GetThemeSachTrongNuoc> themes = new List<GetThemeSachTrongNuoc>();
                var chude = (from c in db.Themes select c).ToList();
                foreach (var cd in chude)
                {
                    GetThemeSachTrongNuoc theme = new GetThemeSachTrongNuoc();
                    theme.themeName = cd.themeName;
                    themes.Add(theme);
                }
                return PartialView("_PartialMenuSachTrongNuoc",themes);
            }
        }
        //Lấy dữ liệu cho partial menu sách nước ngoài
        public PartialViewResult _PartialMenuSachNuocNgoai()
        {
            using (DataContext db = new DataContext())
            {

                List<GetThemeSachNuocNgoai> themes = new List<GetThemeSachNuocNgoai>();
                var chude = (from c in db.Themes select c).ToList();
                foreach (var cd in chude)
                {
                    GetThemeSachNuocNgoai theme = new GetThemeSachNuocNgoai();
                    theme.themeName = cd.themeNameForeign;
                    themes.Add(theme);
                }
                return PartialView("_PartialMenuSachNuocNgoai", themes);
            }
        }
        //Thêm vào giỏ hàng
        [HttpPost]
        public ActionResult Cart(int ? bid)
        {
            using(DataContext db =new DataContext())
            {
                if (bid != null)
                {
                    Book book = db.Books.Where(x => x.Book_ID == bid).FirstOrDefault();
                    if (book.statusSaleOff == true) 
                    {
                        AddToCart(book.Book_ID, book.bookName, book.saleOffPrice,book.image);
                    }
                    else
                    {
                        AddToCart(book.Book_ID, book.bookName, book.price,book.image);
                    }
                    return PartialView("_PartialCart");
                }
                else
                {
                    return PartialView("_Partial404NotFound");
                }
            }
        }
        public void AddToCart(int id, string bookname, int? price, string image)
        { 
            if (Session["Cart"] == null)
            {
                List<Cart_ViewModels> carts = new List<Cart_ViewModels>();
                Cart_ViewModels cart = new Cart_ViewModels();
                cart.book_id = id;
                cart.bookname = bookname;
                cart.image = image;
                cart.number = 1;
                cart.price = price;
                cart.total = Convert.ToDouble(cart.price * cart.number);
                carts.Add(cart);
                Session["cart"] = carts;
            }
            else
            {
                int vitri = -1;
                var carts = (List<Cart_ViewModels>)Session["Cart"];
                for (int i = 0; i < carts.Count; i++)  
                {
                    if (carts[i].book_id == id)
                    {
                        vitri = i;
                    }
                }
                if (vitri == -1)
                {
                    Cart_ViewModels cart = new Cart_ViewModels();
                    cart.book_id = id;
                    cart.bookname = bookname;
                    cart.image = image;
                    cart.number = 1;
                    cart.price = price;
                    cart.total = Convert.ToDouble(cart.price * cart.number);
                    carts.Add(cart);
                }
                else
                {
                    carts[vitri].number++;
                    carts[vitri].total = Convert.ToDouble(carts[vitri].number * carts[vitri].price);
                }
                Session["Cart"] = carts;              
            }
        }
        [HttpPost]
        public ActionResult RemoveItemCart(int ? bid)
        {
            if (bid != null)
            {
                var carts = (List<Cart_ViewModels>)Session["Cart"];
                for (int i = 0; i < carts.Count; i++) 
                {
                    if (carts[i].book_id == bid)
                    {
                        var item = carts[i];
                        carts.Remove(item);
                    }
                }
                Session["Cart"] = carts;
                return PartialView("_PartialCart");
            }
            else
            {
                return PartialView("_Partial404NotFound");
            }
        }
        public ActionResult ViewCart()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UpdateCart(FormCollection f)
        {
            string[] quantity = f.GetValues("quantity");
            var carts = (List<Cart_ViewModels>)Session["Cart"];
            for(int i = 0; i < carts.Count; i++)
            {
                if (Convert.ToInt32(quantity[i]) <= 0)
                {
                    carts.Remove(carts[i]);
                }
                else
                {
                    carts[i].number = Convert.ToInt32(quantity[i]);
                    carts[i].total = Convert.ToDouble(carts[i].number * carts[i].price);
                }
            }
            Session["Cart"] = carts;
            double total = 0;
            foreach(var item in (List<Cart_ViewModels>)Session["Cart"])
            {
                total = total + item.total;
            }
            Session["ThanhTien"] = total;
            return View("ViewCart");
        }
        public ActionResult PaymentWithPaypal(FormCollection f, string Cancel = null)
        {
            //getting the apiContext  
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                //A resource representing a Payer that funds a payment Payment Method as paypal  
                //Payer Id will be returned when payment proceeds or click to pay  
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {      
                    //this section will be executed first because PayerID doesn't exist  
                    //it is returned by the create function call of the payment class  
                    // Creating a payment  
                    // baseURL is the url on which paypal sendsback the data.  
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Home/PaymentWithPayPal?";
                    //here we are generating guid for storing the paymentID received in session  
                    //which will be used in the payment execution  
                    var guid = Convert.ToString((new Random()).Next(100000));
                    //CreatePayment function gives us the payment approval url  
                    //on which payer is redirected for paypal account payment  
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);
                    //get links returned from paypal in response to Create function call  
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment  
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    // saving the paymentID in the key guid  
                    Session.Add(guid, createdPayment.id);
                    using (DataContext db = new DataContext())
                    {
                        Bill bill = new Bill();
                        bill.customerName = f["txtKhachHang"];
                        bill.phoneNumber = f["txtSoDienThoai"];
                        bill.date_set = DateTime.Now;
                        bill.customerAddress = f["txtDiaChi"];
                        bill.total = Convert.ToInt32(Session["ThanhTien"]);
                        bill.isPayment = true;
                        bill.isDelivered = false;
                        db.Bills.Add(bill);
                        db.SaveChanges();
                        var bill_id_max = db.Bills.Max(x => x.Bill_ID);
                        foreach (var item in (List<Cart_ViewModels>)Session["Cart"])
                        {
                            DetailBill detailBill = new DetailBill();
                            detailBill.Bill_ID = bill_id_max;
                            detailBill.Book_ID = item.book_id;
                            detailBill.quantity = item.number;
                            db.DetailBills.Add(detailBill);
                            db.SaveChanges();
                        }
                        Session["Cart"] = null;
                    }
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment  
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("FailureView");
                    }
                }
            }
            catch (Exception ex)
            {
                return View("FailureView");
            }
            //on successful payment, show success page to user.  
            return View("SuccessView");
        }
        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }

        //Tạo thanh toán
        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            //create itemlist and add item objects to it  
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            //Adding Item Details like name, currency, price etc  
            foreach(var s in (List<Cart_ViewModels>)Session["Cart"])
            {
                itemList.items.Add(new Item()
                {
                    name = s.bookname,
                    currency = "USD",
                    price = s.price.ToString(),
                    quantity = s.number.ToString(),
                    sku = "sku"
                });
            }
            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            // Adding Tax, shipping and Subtotal details  
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = Session["ThanhTien"].ToString()
            };
            //Final amount with details  
            var amount = new Amount()
            {
                currency = "USD",
                total = Session["ThanhTien"].ToString(), // Total must be equal to sum of tax, shipping and subtotal.  
                details = details
            };
            var transactionList = new List<Transaction>();
            // Adding description about the transaction  

            //string ramdomTrans = new Guid().ToString();
            char[] chars = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&".ToCharArray();
            Random r = new Random();
            int i = r.Next(chars.Length);

            transactionList.Add(new Transaction()
            {
                description = "Payment Orders",
                invoice_number = chars[i].ToString(), //Generate an Invoice No  
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            // Create a payment using a APIContext  
            return this.payment.Create(apiContext);
        }
        public ActionResult FailureView()
        {
            return View();
        }
        public ActionResult SuccessView()
        {
            return View();
        }
        public ActionResult Bill()
        {
            return View();
        }
        public ActionResult BillPayment(FormCollection f)
        {
            using (DataContext db = new DataContext()) 
            {
                Bill bill = new Bill();
                bill.customerName = f["txtKhachHang"];
                bill.phoneNumber = f["txtSoDienThoai"];
                bill.date_set = DateTime.Now;
                bill.customerAddress = f["txtDiaChi"];
                bill.total = Convert.ToInt32(Session["ThanhTien"]);
                bill.isPayment = false;
                bill.isDelivered = false;
                db.Bills.Add(bill);
                db.SaveChanges();
                var bill_id_max = db.Bills.Max(x => x.Bill_ID);
                foreach (var item in (List<Cart_ViewModels>)Session["Cart"])
                {
                    DetailBill detailBill = new DetailBill();
                    detailBill.Bill_ID = bill_id_max;
                    detailBill.Book_ID = item.book_id;
                    detailBill.quantity = item.number;
                    db.DetailBills.Add(detailBill);
                    db.SaveChanges();
                }
                Session["Cart"] = null;
            }
            return View("SuccessView");
        }
        [HttpGet]
        public ActionResult BooksInCategory(int ? cid)
        {
            if (cid != null)
            {
                using(DataContext db=new DataContext())
                {
                    ViewBag.GetAllCategorys = (from c in db.Categorys select c).ToList();
                    ViewBag.GetBookFromID = (from b in db.Books
                                             join c in db.Categorys 
                                             on b.category_id equals c.Category_ID
                                             where b.category_id == cid select b).ToList();
                    return View();
                }
            }
            else
            {
                return PartialView("_Partial404NotFound");
            }
        }
        //[HttpPost]
        //public ActionResult BooksInCategory(int? cid)
        //{
        //    if (cid != null)
        //    {
        //        using (DataContext db = new DataContext())
        //        {
        //            ViewBag.GetAllCategorys = (from c in db.Categorys select c).ToList();
        //            ViewBag.GetBookFromID = (from b in db.Books
        //                                     join c in db.Categorys
        //                                     on b.category_id equals c.Category_ID
        //                                     where b.category_id == cid
        //                                     select b).ToList();
        //            return View();
        //        }
        //    }
        //    else
        //    {
        //        return PartialView("_Partial404NotFound");
        //    }
        //}
        [HttpGet]
        public ActionResult BookDetail(int ? bid)
        {
            if (bid != null)
            {
                using(DataContext db=new DataContext())
                {
                    ViewBag.GetBook = (from b in db.Books where b.Book_ID == bid select b).ToList();
                    var get_id_Category = (from b in db.Books join c in db.Categorys on b.category_id equals c.Category_ID where b.Book_ID == bid select c.Category_ID).FirstOrDefault();
                    ViewBag.GetBookCategory = (from b in db.Books join c in db.Categorys on b.category_id equals c.Category_ID where c.Category_ID == get_id_Category select b).ToList();
                }        
                return View();
            }
            else
            {
                return PartialView("_Partial404NotFound");
            }
        }

    }
}
