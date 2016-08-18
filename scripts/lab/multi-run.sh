#!/bin/bash

iterations=10
test=plaintext
host=192.168.1.100
port=5000
script=../pipeline.lua
depth=1
connections=512
threads=$(nproc)

while [[ $# > 1 ]]
do
key="$1"

case $key in
    -i|--iterations)
    iterations="$2"
    shift # past argument
    ;;
    -t|--test)
    test="$2"
    shift # past argument
    ;;
    -h|--host)
    host="$2"
    shift # past argument
    ;;
    -p|--port)
    port="$2"
    shift # past argument
    ;;
    --script)
    script="$2"
    shift # past argument
    ;;
    --depth)
    depth="$2"
    shift # past argument
    ;;
    -c|--connections)
    connections="$2"
    shift # past argument
    ;;
    --threads)
    threads="$2"
    shift # past argument
    ;;


esac
shift # past argument or value
done

cmd="./run-wrk.sh -p $port -h $host -t $test --script $script -d 1m --depth $depth -c $connections --threads $threads"
echo $cmd
echo Warming up...
$cmd | grep Requests/sec | awk '{print $2}'
echo Running tests...
cmd="./run-wrk.sh -p $port -h $host -t $test --script $script -d 15s --depth $depth -c $connections --threads $threads"
output=""
for i in `seq 1 $iterations`;
do
    # result="$($cmd | grep Requests/sec | awk '{print $2}')"
    allOutput=$($cmd)
    #result= "$(echo \"$allOutput\" | grep Requests/sec | awk '{print $2}')"
    result=$(echo "$allOutput" | grep Requests/sec | awk '{print $2}')
    echo $result
    echo "$allOutput" | grep "Socket errors"
    echo "$allOutput" | grep "Non-2xx"
    echo "$allOutput" | grep "unable"
    output="$output$result "
done
echo $output
