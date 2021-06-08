#ifndef __IAST_H__
#define __IAST_H__



#include "../IEnumerator.hpp"
#include "AstParser.hpp"



class IAST {
public:
	std::shared_ptr<AstParser> Parser;

	virtual IEnumerator<int> TryParse (int _pos) = 0;

	virtual bool IsValid () = 0;
};



#endif //__IAST_H__
