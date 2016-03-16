using DryIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2 {
  class Program {
    static void Main(string[] args) {
      new Container();


      using (var container = new Container()) {
        var foo = container.Resolve<IContainer>();


      }
    }
  }
}
