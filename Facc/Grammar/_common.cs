using System;

namespace Facc.Grammar {
	enum EbnfExprItemRepeatType { Unknown, _1, _0_to_1, _0_to_N, _1_to_N }
	enum ExprItemListType { Unknown, Any, All }

	static class ExtendMethods {
		public static bool is_1 (this EbnfExprItemRepeatType _t) => _t == EbnfExprItemRepeatType._1;
		public static bool is_01 (this EbnfExprItemRepeatType _t) => _t == EbnfExprItemRepeatType._0_to_1;
		public static bool is_0N (this EbnfExprItemRepeatType _t) => _t == EbnfExprItemRepeatType._0_to_N;
		public static bool is_1N (this EbnfExprItemRepeatType _t) => _t == EbnfExprItemRepeatType._1_to_N;
		public static bool max_1 (this EbnfExprItemRepeatType _t) => _t.is_01 () || _t.is_1 ();
		public static bool max_N (this EbnfExprItemRepeatType _t) => _t.is_0N () || _t.is_1N ();
		public static bool min_0 (this EbnfExprItemRepeatType _t) => _t.is_01 () || _t.is_0N ();
		public static bool min_1 (this EbnfExprItemRepeatType _t) => _t.is_1 () || _t.is_1N ();
	}
}
