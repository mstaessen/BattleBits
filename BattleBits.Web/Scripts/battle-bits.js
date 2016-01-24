angular
    .module('BattleBits', ['ngRoute'])
    .controller('LeaderboardController', function() {

    })
    .controller('GamePlayController', function ($scope, $interval) {
        var timer;
        $scope.guess = 0;
        // TODO load from server
        // NOTE: can one of the HEXs be zero? in that case the printing is sometimes wrong
        $scope.numbers = [21, 59, 192, 204, 153, 99, 66, 12, 199, 200, 222, 250, 21, 59, 192, 204, 153, 99, 66, 12, 199, 200, 222, 250];
        $scope.numbersGuessed = 0;
        $scope.numbersToGuess = $scope.numbers.length;
        $scope.timeLeft = 45;
        $scope.number = $scope.numbers[$scope.numbersGuessed];

        $scope.isBitActive = function (bitPosition) {
            return ($scope.guess & (1 << bitPosition)) !== 0;
        };

        $scope.toggleBit = function(bitPosition) {
            $scope.guess ^= (1 << bitPosition);
        };

        $scope.$watch('guess', function (newValue) {
            if ($scope.timeLeft <= 0) {
                // TODO no more guesses when time is expired (this may be placed somewhere else?)
                return;
            }
            if (newValue === $scope.number) {
                // TODO handle correct answer to server
                if (++$scope.numbersGuessed === $scope.numbersToGuess) {
                    $interval.cancel(timer);
                    // TODO Notify that all bits are solved (show results)
                } else {
                    // assign next number
                    $scope.number = $scope.numbers[$scope.numbersGuessed];
                    $scope.guess = 0;
                }
            }
        });

        timer = $interval(function() {
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