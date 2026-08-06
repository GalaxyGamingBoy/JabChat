#! /usr/bin/env sh

VERSION=3530300
echo "Updating sqlite to version $VERSION"
echo "Press enter to continue"
read

wget "https://sqlite.org/2026/sqlite-wasm-$VERSION.zip" -O sqlite.zip
unzip sqlite.zip "sqlite-wasm-$VERSION/jswasm/*" -d wwwroot

cd wwwroot

rm -r jswasm
rm slqite3.js
rm sqlite3.wasm
rm sqlite3-worker1.js
rm sqlite3-worker1-promiser.js

mv "sqlite-wasm-$VERSION/jswasm" .
rmdir "sqlite-wasm-$VERSION"

mv jswasm/sqlite3.wasm .
mv jswasm/sqlite3.js .
mv jswasm/sqlite3-worker1.js .
mv jswasm/sqlite3-worker1-promiser.js .

cd ..
rm sqlite.zip

echo "Done"