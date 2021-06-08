#ifndef __IENUMERATOR_HPP__
#define __IENUMERATOR_HPP__



#include <experimental/coroutine>



template<typename T>
class IEnumerator {
public:
	struct promise_type;
	using _Hty = std::experimental::coroutine_handle<promise_type>;
	struct promise_type {
		IEnumerator<T> get_return_object () { return IEnumerator<T> { 0, _Hty::from_promise (*this), true }; }
		void unhandled_exception () { std::terminate (); }
		auto initial_suspend () { return std::experimental::suspend_never {}; }
		auto final_suspend () { return std::experimental::suspend_always {}; }
		void return_void () {}
		auto yield_value (const T &val) {
			_Val = val;
			return std::experimental::suspend_always {};
		}
		T _Val;
	};
	bool await_ready () const { return false; }
	void await_suspend () {}
	bool MoveNext () {
		if (_init) {
			_init = false;
		} else {
			_Handle.resume ();
		}
		bool _ret = !_Handle.done ();
		if (_ret)
			Current = _Handle.promise ()._Val;
		return _ret;
	}

	T Current;
	_Hty _Handle;
	bool _init = true;
};



#endif // __IENUMERATOR_HPP__
