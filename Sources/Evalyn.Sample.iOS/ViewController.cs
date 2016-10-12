using System;
using System.Linq;
using UIKit;

using Microsoft.CodeAnalysis.Symbols;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq.Expressions;
using Evalyn;

namespace Evalyn.Sample.iOS
{
	public class Foo
	{
		int g = 5;

		public void Bar(int x, Foo y)
		{
			var s = new string('W',1);
			var t = 10 * x;
			System.Diagnostics.Debug.WriteLine(s + t.ToString());
		}
	}

	public partial class ViewController : UIViewController
	{
		protected ViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		private string[] References =
		{
			typeof(object).Assembly.Location,
			typeof(Enumerable).Assembly.Location,
			typeof(System.Diagnostics.Debug).Assembly.Location,
		};

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			// Perform any additional setup after loading the view, typically from a nib.
		

			var code = @"
			using System;

			public class Other
			{
				public int Prop { get; set; }
			}

			public class Doo
			{
				public virtual int Woo(int t) => t + 15;
			}

			public class Foo : Doo
			{
				public Foo()
				{
					this.Prop = 5;
				}

				public int Field;

				public int Prop { get; set; }

				public int ROnly => this.Prop;

				public int Test(int x)
				{
					System.Diagnostics.Debug.WriteLine(""X:""+x);
					return 10 * x;
				}

				public override int Woo(int t) => base.Woo(t) + 100;
				
			    public int Main(int x) 
				{ 
					var array = new int[5];
					var array1 = new int[] { 4, 5, 6, 7};
					var array2 = new int[4] { 4, 5, 6, 7};
					var array3 = new [] { 4, 5, 6, 7};
					const int R = 40;
					var s = new string('W',1);
					int t = Test(x);
					
					Doo doo = new Foo();
					System.Diagnostics.Debug.WriteLine(nameof(doo) + ""->"" + doo.Woo(5));
					
					//t = ++t;

					this.Prop += R < 20 ? this.Prop + 2 : 10;
					this.Field = this.Field + 3;
					this.Field = this.Woo(this.Field);

					System.Diagnostics.Debug.WriteLine(s + ""->Prop"" + this.Prop + ""->t"" + t.ToString());
					System.Diagnostics.Debug.WriteLine(nameof(this.Field) + ""->"" + this.Field);
					System.Diagnostics.Debug.WriteLine(nameof(array) + ""->"" + array);
					System.Diagnostics.Debug.WriteLine(nameof(array1) + ""->"" + array1);
					System.Diagnostics.Debug.WriteLine(nameof(array2) + ""->"" + array2);
					System.Diagnostics.Debug.WriteLine(nameof(array3) + ""->"" + array3);

					try
					{
						var o = 5 / t;
					}
					catch(Exception e)
					{
						System.Diagnostics.Debug.WriteLine(e.Message);
					}

					Action<int> lambda1 = y => x = x + y;
					Func<int,int> lambda = y => y * y;
					lambda1(2);
					return lambda(t);
				}
			}";

			var interpreter = new Interpreter(References);

			var context = interpreter.CreateContext(code);

			var walker = new FileWalker(context);
			walker.Build(context.DebugTree.GetRoot());

			var Foo = context.ResultAssembly.GetType("Foo");
			var Main = Foo.GetMethod<int>("Main");
			var Constructor = Foo.GetMethod(".ctor");

			var foo = new EvalInstance(Foo,Constructor);

			var result = Main.Invoke(foo, 40);

			var label = new UILabel();
			label.Frame = this.View.Frame;
			label.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
			label.Text = $"{result}";
			this.View.AddSubview(label);
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}
