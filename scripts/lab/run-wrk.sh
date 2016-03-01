#!/bin/bash
threads=`nproc`
host=192.168.1.100
port=5000
test=plaintext
connections=512
duration=60
depth=16
script=../pipeline.lua
while [[ $# > 1 ]]
do
key="$1"

case $key in
    -h|--host)
    host="$2"
    shift # past argument
    ;;
    -p|--port)
    port="$2"
    shift # past argument
    ;;
    -t|--test)
    test="$2"
    shift # past argument
    ;;
    -c|--connections)
    connections="$2"
    shift # past argument
    ;;
    -d|--duration)
    duration="$2"
    shift # past argument
    ;;
    --depth)
    depth="$2"
    shift # past argument
    ;;
    --script)
    script="$2"
    shift # past argument
    ;;
    --threads)
    threads="$2"
    shift # past argument
    ;;

esac
shift # past argument or value
done

wrk --latency -c $connections -t $threads -d $duration --timeout 8 -s $script http://$host:$port/$test -- $depth
