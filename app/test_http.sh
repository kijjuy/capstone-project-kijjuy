#!/bin/bash

#Constants
RED='\033[0;31m'
NC='\033[0m'
endpoint=http://localhost:8080

increment_passed() {
    passed_count=$(($passed_count+1))
}

increment_failed() {
    failed_count=$(($failed_count+1))
}

print_error() {
   printf "\n${RED}${1}${NC}\n\n"
}

passed_count=0
failed_count=0

#start .net server
dotnet run > /dev/null 2>&1 &
dotnet_pid=$!
echo dotnet pid=$dotnet_pid
#sleep for 5 seconds to wait for server to start... could try checking some endpoint to see if results are found...
echo Waiting for .net to start...
sleep 5

echo starting tests to $endpoint

#test creating product
result=$(curl -s $endpoint/products -X POST -H "Content-Type: x-www-form-urlencoded" \
    -d "Name=NameFromTestScript&CategoryId=1&Price=123.45&Description=test description")
echo $result | jq .message > /dev/null 2>&1

# jq returns 0 if it successfully finds .messages key, which is bad.
if [[ $? == 0 ]]; then
    print_error "Error adding product to database."
    increment_failed
else
    increment_passed
fi


#test getting products
result=$(curl -s $endpoint/products | jq length)

if [[ $result < 1 ]]; then
    print_error "Error getting products from database."
    increment_failed
else
    increment_passed
fi


#kill .net server
echo Killing .net server
kill $dotnet_pid


#echo final results
echo "Results: Passed=$passed_count; Failed=$failed_count"
