angular
    .module('BattleBitsPractice', ['ngRoute'])
    .controller('GamePracticeController', ['$scope', function ($scope) {
        $scope.isBitActive = function (bitPosition) {
            return ($scope.guess & (1 << bitPosition)) !== 0;
        };
        $scope.nextNumber = function (max) {
            return Math.floor(Math.random() * max + 1);
        }
        $scope.nextByte = function () {
            return ($scope.nextNumber(15) << 4) + $scope.nextNumber(15);
        }
        $scope.toggleBit = function(bitPosition) {
            $scope.guess ^= (1 << bitPosition);
        };
        $scope.guess = 0;
        $scope.numbersGuessed = 0;
        $scope.number = $scope.nextByte();

        var numberWatcher = $scope.$watch('guess', function(newValue) {
            if (newValue === $scope.number) {
                $scope.numbersGuessed++;
                $scope.guess = 0;
                $scope.number = $scope.nextByte();
            }
        });
    }])
    .filter('binary', function () {
        return function(input, length) {
            length = length || 8;
            return _.padStart((input >>> 0).toString(2), length, '0');
        };
    })
    .filter('hex', function() {
        return function(input) {
            return "0x" + input.toString(16);
        };
    });