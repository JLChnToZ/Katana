Katana
======
Katana is a newly-designed light-weight programming language and also a tree data structure expression. It aims to keep the parser simple, and highly inspired S-Expression, M-Expression and JSON format.

The parser, encoder and runner in this repository is still in early development and not ready for use.

Example
-------
The language itself is look like this, somewhat looks like a mixture of LISP and JavaScript:
```
function(factorial, (x), $(
  =(c, 1),
  =(result, 1),
  while(<=(c, n), $(
    =(result, *(@(result), @(c))),
    =(c, +(@(c), 1))
  )),
  return(result)
)),
function(main, (), $(
  print("please enter the number"),
  readNumber(@(number)),
  =(result, factorial(@(number))),
  print(format("factorial of %d: %d", @(number), @(result))),
  return(0)
))
```

This expression can be used to represent a tree structure too:
```
root(
  child(
    another child,
    hello!
  ),
  3.1415,
  false,
  nil
)
```

License
-------
[MIT](LICENSE)