#!/bin/sh

SCRIPT_DIR=$(cd $(dirname $0); pwd)
BUILD_METHOD=$1

BUILD_PATH=$SCRIPT_DIR/Build/${BUILD_METHOD}
LOG_PATH=$SCRIPT_DIR/Build/${BUILD_METHOD}_stdlog

mkdir -p $BUILD_PATH

cd $SCRIPT_DIR && /Applications/Unity/Hub/Editor/2021.2.4f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -logfile $LOG_PATH -projectPath ./ -buildPath $BUILD_PATH -executeMethod ScriptedBuilds.${BUILD_METHOD} -quit
