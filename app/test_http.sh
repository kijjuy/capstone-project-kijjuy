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
result=$(curl -s $endpoint/products -X POST -H "Content-Type: application/x-www-form-urlencoded" \
    -d "Name=NameFromTestScript&CategoryId=1&Price=123.45&Description=test description")
jq_result=$(echo $result | jq .message)

#if result != null, then .message was found, which is bad
if [[ "$jq_result" != "null" ]]; then
    print_error "Error adding product to database."
    echo result=$result
    increment_failed
else
    echo added product to database with $result
    increment_passed
fi


#test getting new single product
new_id=$(echo $result | jq .id)
result=$(curl -s $endpoint/products/$new_id)
jq_result=$(echo $result | jq .productId)
if [[ $jq_result != $new_id ]]; then
    print_error "Error getting new product from database"
    echo result=$result
    increment_failed
else
    echo got new product successfully
    increment_passed
fi


#test getting products
result=$(curl -s $endpoint/products)
jq_result=$(echo $result | jq .[-1].ProductId)
if [[ $jq_result != $new_id ]]; then
    print_error "Error getting products from database."
    echo result is...
    echo result=$result
    increment_failed
else
    increment_passed
fi

#test deleting product
result=$(curl -s $endpoint/products/$new_id -X DELETE)
jq_result=$(echo $result | jq .message)
echo jq_result=$jq_result
if [[ $jq_result != "null" ]]; then
    print_error "Error deleting product from database."
    echo result=$result
    increment_failed
else
    echo deleted new product from database
    increment_passed
fi




#kill .net server
echo Killing .net server
kill $dotnet_pid


#echo final results
echo "Results: Passed=$passed_count; Failed=$failed_count"
