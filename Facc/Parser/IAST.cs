using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Facc.Parser {
	public interface IAST {
		AstParser Parser { init; get; }

		public IEnumerator<int> TryParse (int _pos);

		bool IsValid ();
	}
}
