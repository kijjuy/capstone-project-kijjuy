# !/bin/bash

first_migration=001-init.sql
test_insert=test_insert.sql
test_update=test_update.sql
test_select=test_select.sql

check_return() {
    if [[ $? != 0 ]]; then
	echo "Error $1 test data."
	exit 1
    fi
}

sqlite3 test.db < $first_migration

check_return "migrating"
echo "Migration successful."

sqlite3 test.db < $test_insert

check_return "inserting"
echo "Inserting successful."

mkdir tmp

#pipe select into 1.diff
sqlite3 test.db < $test_select > tmp/1.diff
check_return "selecting"

echo "sleeping for one second..."
sleep 1

#pipe select into 2.diff
sqlite3 test.db < $test_update
check_return "updating"

sqlite3 test.db < $test_select > tmp/2.diff
check_return "selecting"

diff=$(diff tmp/1.diff tmp/2.diff)
if [[ $? == 0 ]]; then
    echo "[ERROR]: No difference after updating."
fi

read -ra diff_res <<< "$diff"


if [[ $diff_res == "1,2c1,2" ]]; then
    echo "[ERROR]: Both lines updated."
fi

#rm -r tmp

