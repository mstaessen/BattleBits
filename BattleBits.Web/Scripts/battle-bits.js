angular
    .module('BattleBits', ['ngRoute'])
    .controller('LeaderboardController', function() {

    })
    .controller('GamePlayController', function ($scope, $interval) {
        $scope.guess = 0;
        $scope.number = 57;
        $scope.numbersGuessed = 0;
        $scope.numbersToGuess = 24;
        $scope.timeLeft = 45;

        $scope.isBitActive = function (bitPosition) {
            var mask = (1 << bitPosition);
            return ($scope.guess & mask) === mask;
        };

        $scope.toggleBit = function(bitPosition) {
            $scope.guess ^= (1 << bitPosition);
        };

        var numberWatcher = $scope.$watch('number', function(newValue) {
            // Send Guess, if OK, move to next number
        });

        $interval(function() {
            $scope.timeLeft--;
        }, 1000, 45);
    })
    .controller('GameDisplayController', function($scope) {

    })
    .service('GameService', function() {

    })
    .filter('binary', function() {
        return function (input, length) {
            length = length || 8;
            return _.padStart((input >>> 0).toString(2), length, '0');
        };
    })
    .filter('hex', function() {
        return function (input) {
            return "0x" + input.toString(16);
        };
    });