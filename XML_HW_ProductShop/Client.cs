using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using XML_HW_ProductShop.Data;
using XML_HW_ProductShop.Models;

namespace XML_HW_ProductShop
{
    class Client
    {
        public static void Main(string[] args)
        {
            var context = new ProductShopContext();

            //context.Database.Initialize(true);

            //ImportUsers(context);
            //ImportProducts(context);
            //ImportCategories(context);

            //GetProductsInRange(context);

            //GetSoldProducts(context);

            // GetAllCategories(context);

            GetUsersAndProducts(context);

        }

        private static void GetUsersAndProducts(ProductShopContext context)
        {
            var listOfUser = context.Users
                .Where(user => user.ProductsSold.Count() >= 1)
                .OrderByDescending(soldP=>soldP.ProductsSold.Count)
                .ThenBy(uLastName=>uLastName.LastName)
                .Select(user => new

                {
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    age = user.Age,
                    soldPoducts = new

                    {
                        count = user.ProductsSold.Count(),

                        products = user.ProductsSold.Select(product => new
                        {
                            product.Name,
                            product.Price

                        })

                    }

                }).ToList();

            var counts = new
            {
                usersCount = listOfUser.Count(),
                users = listOfUser
            };

            XDocument Doc = new XDocument();

            XElement usersXML = new XElement("users");
            usersXML.SetAttributeValue("count", listOfUser.Count);

            foreach (var user in listOfUser)
            {
                XElement userXML = new XElement("user");
                userXML.SetAttributeValue("first-name", user.firstName);
                userXML.SetAttributeValue("last-name", user.lastName);
                userXML.SetAttributeValue("age", user.age);

                XElement soldProductsXML = new XElement("sold-products");
                soldProductsXML.SetAttributeValue("count", user.soldPoducts.count);

                foreach (var prod in user.soldPoducts.products)
                {
                    XElement productXML = new XElement("product");
                    productXML.SetAttributeValue("name", prod.Name);
                    productXML.SetAttributeValue("price", prod.Price);

                    soldProductsXML.Add(productXML);
                }

                userXML.Add(soldProductsXML);
                usersXML.Add(userXML);
            }
            Doc.Add(usersXML);
            Doc.Save(@"c:\users\stoyadim\documents\visual studio 2015\Projects\XML_HW_ProductShop\XML_HW_ProductShop\Results\04.UsersAndProducts");
        }

        private static void GetAllCategories(ProductShopContext context)
        {
            var listOfCategories = context.Categories
               .OrderBy(cname => cname.Name)
               .Select(c => new
               {
                   category = c.Name,
                   productsCount = c.Products.Count(),
                   averagePrice = c.Products.Average(product => product.Price),
                   totalRevenue = c.Products.Sum(p => p.Price)

               });
            XDocument Doc = new XDocument();

            XElement categoriesXml = new XElement("categories");

            foreach ( var cat in listOfCategories)
            {
                XElement categoryXml = new XElement("category");
                categoryXml.SetAttributeValue("name", cat.category);

               
                    XElement productsCountXml = new XElement("products-count", cat.productsCount);
                    XElement averagePriceXml = new XElement("average-price", cat.averagePrice);
                    XElement totalRevenueXml = new XElement("total-revenue", cat.totalRevenue);

                    categoryXml.Add(productsCountXml);
                    categoryXml.Add(averagePriceXml);
                    categoryXml.Add(totalRevenueXml);
                
                categoriesXml.Add(categoryXml);
            }
            Doc.Add(categoriesXml);
            Doc.Save(@"c:\users\stoyadim\documents\visual studio 2015\Projects\XML_HW_ProductShop\XML_HW_ProductShop\Results\03.CategoriesCount");


        }

        private static void GetSoldProducts(ProductShopContext context)
        {
            var listOfProducts = context.Users
                .Where(user => user.ProductsSold.Count(product => product.Buyer != null) != 0)
                .OrderBy(user => user.LastName)
                .ThenBy(user => user.FirstName)
                .Select(user => new
                {
                    user.FirstName,
                    user.LastName,
                    soldProducts = user.ProductsSold.Select(product => new
                    {
                        product.Name,
                        product.Price,
                        buyerFirstName = product.Buyer.FirstName,
                        buyerLastName = product.Buyer.LastName
                    })
                });



            XDocument Doc = new XDocument();

            XElement users = new XElement("users");

            foreach (var user in listOfProducts)
            {
                XElement userElement = new XElement("user");
                userElement.SetAttributeValue("first-name", user.FirstName);
                userElement.SetAttributeValue("last-name", user.LastName);

                XElement soldProductElement = new XElement("sold-products");

                foreach (var prod in user.soldProducts)
                {
                    XElement productElement = new XElement("product");
                    productElement.SetElementValue("name", prod.Name);
                    productElement.SetElementValue("price", prod.Price);

                    soldProductElement.Add(productElement);
                }
                userElement.Add(soldProductElement);
                users.Add(userElement);
            }
            Doc.Add(users);

            Doc.Save(@"c:\users\stoyadim\documents\visual studio 2015\Projects\XML_HW_ProductShop\XML_HW_ProductShop\Results\1.xml");
        }

        private static void GetProductsInRange(ProductShopContext context)
        {
            var productsInRange = context.Products
                .Where(p => p.Price >= 1000 && p.Price <= 2000 && p.BuyerId != null)
                .Select(u => new
                {
                    name = u.Buyer.FirstName + " " + u.Buyer.LastName,
                    price = u.Price,
                    productName = u.Name
                });

            XElement products = new XElement("products");

            foreach (var p in productsInRange)
            {
                XElement product = new XElement("product");
                product.SetAttributeValue("name", p.productName);
                product.SetAttributeValue("price", p.price);
                product.SetAttributeValue("buyer", p.name);
                products.Add(product);
            }
            products.Save(@"c:\users\stoyadim\documents\visual studio 2015\Projects\XML_HW_ProductShop\XML_HW_ProductShop\Results\ProductsInRange.xml");
           
        }


        private static void ImportCategories(ProductShopContext context)
        {
            XDocument categoriesDocs = XDocument.Load(@"c:\users\stoyadim\documents\visual studio 2015\Projects\XML_HW_ProductShop\XML_HW_ProductShop\Imports\products.xml");

            XElement categoriesRoot = categoriesDocs.Root;
            Random rnd = new Random();
            List<Category> categories = new List<Category>();
            int countOfProducts = context.Products.Count();

            foreach (XElement categoryElement in categoriesRoot.Elements())
            {
                Category category = new Category()
                {
                    Name = categoryElement.Element("name").Value,

                };
                for (int i = 0; i < countOfProducts; i++)
                {

                    Product product = context.Products.Find(rnd.Next(1, countOfProducts + 1));
                    category.Products.Add(product);


                }
                context.Categories.Add(category);

            }
            context.SaveChanges();
        }

        

        private static void ImportProducts(ProductShopContext context)
    {
        XDocument productsDocs = XDocument.Load(@"c:\users\stoyadim\documents\visual studio 2015\Projects\XML_HW_ProductShop\XML_HW_ProductShop\Imports\products.xml");

        XElement productsRoots = productsDocs.Root;

        Random rnd = new Random();
        int countOfUsers = context.Users.Count();
        foreach (var productElement in productsRoots.Elements())
        {
            string name = productElement.Element("name")?.Value;
            decimal price = decimal.Parse(productElement.Element("price")?.Value);

            Product product = new Product()
            {
                Name = name,
                Price = price,

            };
            int sellerID = rnd.Next(1, countOfUsers);

            product.SelledId = sellerID;

            if (sellerID % 3 == 0)
            {
                int buyerID = rnd.Next(1, countOfUsers);
                product.BuyerId = buyerID;
            }

            context.Products.Add(product);
        }
        context.SaveChanges();

    }
    private static void ImportUsers(ProductShopContext context)
    {
        XDocument usersDoc = XDocument.Load(@"c:\users\stoyadim\documents\visual studio 2015\Projects\XML_HW_ProductShop\XML_HW_ProductShop\Imports\users.xml");

        XElement usersRoot = usersDoc.Root;

        foreach (XElement UserElement in usersRoot.Elements())
        {
            string firstName = UserElement.Attribute("first-name")?.Value;
            string lastName = UserElement.Attribute("last-name")?.Value;
            int age = 0;
            if (UserElement.Attribute("age") != null)
            {
                age = int.Parse(UserElement.Attribute("age")?.Value);
            }


            User user = new User()
            {
                FirstName = firstName,
                LastName = lastName,
                Age = age
            };
            context.Users.Add(user);
        }
        context.SaveChanges();
    }
}
}
