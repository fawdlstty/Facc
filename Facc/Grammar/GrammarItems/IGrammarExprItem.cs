using System;

namespace Facc.Grammar.GrammarItems {
	interface IGrammarExprItem {
		EbnfExprItemRepeatType RepeatType { get; set; }
		string Suffix { get; set; }
		string GetClearStrings ();
		string GenerateTryParse2 ();
		string IsValidExpr ();
		string PrintTree ();
		string MValue ();
		string LengthExpr ();
	}
}
