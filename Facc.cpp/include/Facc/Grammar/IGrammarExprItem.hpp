#ifndef __IGRAMMAR_EXPR_ITEM_HPP__
#define __IGRAMMAR_EXPR_ITEM_HPP__



#include <string>



enum class EbnfExprItemRepeatType { Unknown, _1, _0_to_1, _0_to_N, _1_to_N };
class RType {
public:
	static bool is_1 (EbnfExprItemRepeatType _t) { return _t == EbnfExprItemRepeatType::_1; }
	static bool is_01 (EbnfExprItemRepeatType _t) { return _t == EbnfExprItemRepeatType::_0_to_1; }
	static bool is_0N (EbnfExprItemRepeatType _t) { return _t == EbnfExprItemRepeatType::_0_to_N; }
	static bool is_1N (EbnfExprItemRepeatType _t) { return _t == EbnfExprItemRepeatType::_1_to_N; }
	static bool max_1 (EbnfExprItemRepeatType _t) { return is_01 (_t) || is_1 (_t); }
	static bool max_N (EbnfExprItemRepeatType _t) { return is_0N (_t) || is_1N (_t); }
	static bool min_0 (EbnfExprItemRepeatType _t) { return is_01 (_t) || is_0N (_t); }
	static bool min_1 (EbnfExprItemRepeatType _t) { return is_1 (_t) || is_1N (_t); }
};

enum class ExprItemListType { Unknown, Any, All };

class IGrammarExprItem {
public:
	EbnfExprItemRepeatType RepeatType = EbnfExprItemRepeatType::_1;
	std::string Suffix;
	virtual void SetSuffix (std::string _suffix) { Suffix = _suffix; }
	virtual std::string GetClearStrings () = 0;
	virtual std::string GenerateTryParse2 (std::string _parent_class_name) = 0;
	virtual std::string IsValidExpr () = 0;
	virtual std::string PrintTree () = 0;
	virtual std::string MValue () = 0;
};



#endif //__IGRAMMAR_EXPR_ITEM_HPP__
