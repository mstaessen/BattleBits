angular
    .module('BattleBits', ['ngRoute'])
    .controller('LeaderboardController', function() {

    })
    .controller('GamePlayController', function($scope, $interval, BattleBitsService) {
        $scope.guess = 0;
        // TODO load from server
        // NOTE: can one of the HEXs be zero? in that case the printing is sometimes wrong
        $scope.numbers = [21, 59, 192, 204, 153, 99, 66, 12, 199, 200, 222, 250, 21, 59, 192, 204, 153, 99, 66, 12, 199, 200, 222, 250];
        $scope.numbersGuessed = 0;
        $scope.numbersToGuess = $scope.numbers.length;
        $scope.timeLeft = 45;
        $scope.number = $scope.numbers[$scope.numbersGuessed];

        $scope.isBitActive = function(bitPosition) {
            return ($scope.guess & (1 << bitPosition)) !== 0;
        };

        $scope.toggleBit = function(bitPosition) {
            $scope.guess ^= (1 << bitPosition);
        };

        var timer = $interval(function() {
            $scope.timeLeft--;
        }, 1000, 45);

        var numberWatcher = $scope.$watch('guess', function(newValue) {
            if (newValue === $scope.number) {
                BattleBitsService
                    .guess($scope.numbersGuessed, newValue)
                    .then(function(nextNumber) {
                        if (++$scope.numbersGuessed === $scope.numbersToGuess) {
                            $interval.cancel(timer);
                            // TODO Notify that all bits are solved
                        } else {
                            // assign next number
                            $scope.number = $scope.numbers[$scope.numbersGuessed];
                            $scope.guess = 0;
                        }
                    }, function() {

                    });
            }
        });
    })
    .controller('GameDisplayController', function($scope) {

    })
    .service('BattleBitsService', function (Competition, BattleBitsHub, $q) {

        return {
            joinCompetition: function(competitionId) {
                var deferred = $q.defer();
                BattleBitsHub.server.joinCompetition(competitionId)
                    .done(function() {
                        deferred.resolve();
                    })
                    .fail(function(error) {
                        deferred.reject(error);
                    });
                return deferred.promise;
            },
            joinGame: function() {

            },
            leaveGame: function() {

            },
            guess: function(number, guess) {

            }
        };
    })
    .filter('binary', function() {
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